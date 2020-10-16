using Keyboardchat.SaveCollections;
using System.Collections;
using System.Collections.Generic;

namespace Keyboardchat.Models
{
    public class Room
    {
        public string Name { get; protected set; }
        public string Password { get; protected set; }
        public SaveList<User> Users { get; protected set; }

        public Room(string Name, string Password) : this(Name, Password, new SaveList<User>())
        {          
        }

        private Room(string Name, string Password, SaveList<User> users) 
        {
            this.Name = Name;
            this.Password = Password;
            Users = users;
        }

        public void AddUser(User user)
        {
            var Interface = Users.EnterInQueue();

            Interface.Add(user);

            Users.ExitFromQueue(Interface);
        }

        public void DeleteUser(User user)
        {
            var Interface = Users.EnterInQueue();

            Interface.Remove(user);

            Interface.ExitFromQueue();
        }

        public Room Copy()
        {
            return new Room(new string(Name), new string(Password), Users);
        }

    }
}
