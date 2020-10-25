using Keyboardchat.SaveCollections;
using System.Collections;
using System.Collections.Generic;

namespace Keyboardchat.Models
{
    public class Room
    {
        public string Name { get; protected set; }
        public string Password { get; protected set; }
        public List<User> Users { get; protected set; }

        public Room(string Name, string Password) : this(Name, Password, new List<User>())
        {          
        }

        private Room(string Name, string Password, List<User> users) 
        {
            this.Name = Name;
            this.Password = Password;
            Users = users;
        }

        public void AddUser(User user)
        {  
            if(!Users.Contains(user))
                Users.Add(user);
        }

        public void DeleteUser(User user)
        {
            Users.Remove(user);
        }

        public Room Copy()
        {
            return new Room(new string(Name), new string(Password), Users);
        }

    }
}
