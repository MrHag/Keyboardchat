using Keyboardchat.SaveCollections;
using System.Collections;
using System.Collections.Generic;

namespace Keyboardchat.Models
{
    public class Room
    {
        public int Id { get; protected set; }
        public string Name { get; protected set; }
        public string Password { get; protected set; }
        public List<User> Users { get; protected set; }

        public Room(int Id, string Name, string Password) : this(Id, Name, Password, new List<User>())
        {          
        }

        private Room(int Id, string Name, string Password, List<User> users) 
        {
            this.Id = Id;
            this.Name = Name;
            this.Password = Password;
            Users = users;
        }

        public void AddUser(User user)
        {
            lock (Users)
            {
                if (!Users.Contains(user))
                    Users.Add(user);
            }
        }

        public void DeleteUser(User user)
        {
            lock (Users)
            {
                Users.Remove(user);
            }
        }

    }
}
