using System;
using System.Collections.Generic;

namespace AkkaChat.Modules
{
    using Nancy;

    public class IndexModule : NancyModule
    {
        public IndexModule()
        {
            Get["/"] = _ =>
            {
                // TODO: remove all this boilerplate
                List<User> users = new List<User>();
                for (int i = 0; i < 50; i++)
                {
                    users.Add(new User(Faker.Name.FullName()));
                }

                List<string> messages = new List<string>();
                for (int i = 0; i < 150; i++)
                {
                    messages.Add(Faker.Lorem.Paragraph());
                }
                ViewBag.Messages = messages;


                List<string> rooms = new List<string>();
                for (int i = 0; i < Faker.RandomNumber.Next(1,25); i++)
                {
                    rooms.Add(string.Format("chat room {0}", i));
                }
                ViewBag.Rooms = rooms;
                return View["index", users];
            };
        }
    }

    public class User
    {
        public User(string name)
        {
            Name = name;
            UserId = Guid.NewGuid();
        }

        public string Name { get; set; }
        public Guid UserId { get; set; }
    }
}