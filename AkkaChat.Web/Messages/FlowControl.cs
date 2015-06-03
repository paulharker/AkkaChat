using Akka.Actor;

namespace AkkaChat.Messages
{
    /// <summary>
    /// Implements part of the CONSENSUS pattern.
    /// 
    /// Used as part of flow-control where consesus or permission is
    /// needed to proceed with some operation.
    /// 
    /// <remarks>
    /// This illustrates a naive implementation.
    /// 
    /// Full-blown distributed consensus is often much more complicated
    /// and is an area of intense academic research.
    /// </remarks>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CanProceed<T>
    {
        public CanProceed(T originalMessage, IActorRef intendedDestination, bool canExecute)
        {
            CanExecute = canExecute;
            IntendedDestination = intendedDestination;
            OriginalMessage = originalMessage;
        }

        public T OriginalMessage { get; private set; }

        public IActorRef IntendedDestination { get; private set; }

        public bool CanExecute { get; private set; }
    }
}