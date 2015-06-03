using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Persistence;
using Akka.Util.Internal;
using AkkaChat.Data.Models;
using AkkaChat.Messages;

namespace AkkaChat.Actors
{
    /// <summary>
    /// PERSISTENT ACTOR.
    /// 
    /// Responsible for managing a single chatroom's message history.
    /// </summary>
    public class RoomMessageHistoryActor : ReceivePersistentActor
    {
        #region Internal classes

        /// <summary>
        /// In-memory EventStore for the message history for a given chatroom
        /// </summary>
        public class MessageHistory
        {
            public MessageHistory(IEnumerable<ChatMessage> messages = null)
            {
                Messages = new SortedSet<ChatMessage>(messages ?? new List<ChatMessage>());
            }

            public IEnumerable<ChatMessage> Messages { get; private set; }

            public bool WillUpdate(ChatMessage message)
            {
                return !Messages.Contains(message);
            }

            public MessageHistory Update(ChatMessage message)
            {
                return new MessageHistory(Messages.Concat(message));
            }
        }

        #endregion

        private readonly ExistingRoom _room;
        
        private MessageHistory _history;

        private string _cachedPersistenceId;
        public override string PersistenceId
        {
            get
            {
                if (string.IsNullOrEmpty(_cachedPersistenceId))
                {
                    _cachedPersistenceId = string.Format("room-{0}-messages", _room.Id);
                }
                return _cachedPersistenceId;
            }
        }

        private readonly IActorRef _signalRWriter;

        public RoomMessageHistoryActor(ExistingRoom room, IActorRef signalRWriter)
        {
            _signalRWriter = signalRWriter;
            _room = room;
            _history = new MessageHistory();
            ReadyCommands();
            ReadyRecovers();
        }

        private void ReadyCommands()
        {
            //SystemMessages don't get added to the message history
            Command<SystemMessage>(message =>
            {
                _signalRWriter.Forward(message);
            });

            //UserMessages do get persisted
            Command<UserMessage>(message => Persist(message, UpdateHistory));

            //Need to catch specific SignalR user up on the messages
            Command<FetchPreviousMessages>(previous =>
            {
                var queryTime = previous.Since ?? DateTime.MaxValue;
                var messages = _history.Messages.Where(x => x.When <= queryTime).OrderBy(x => x.When).Take(30).ToList();
                _signalRWriter.Tell(new HistoricalMessageBatch(messages, previous.RoomName, previous.ConnectionId));
            });
        }

        private void UpdateHistory(ChatMessage message)
        {
            if (!_history.WillUpdate(message)) return;
            _signalRWriter.Forward(message);
            _history = _history.Update(message);
        }

        private void ReadyRecovers()
        {
            Recover<UserMessage>(message => RecoverHistory(message));
        }

        private void RecoverHistory(ChatMessage message)
        {
            _history = _history.Update(message);
        }
    }
}