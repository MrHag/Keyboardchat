using System.Collections.Generic;

namespace KeyBoardChat.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; protected set; }
        public string Password { get; protected set; }

        private List<User> _users;
        public IEnumerable<User> Users { get { return _users; } }

        public event DataDelegate<User> UserJoined;

        public event DataDelegate<User> UserLeaved;

        public Room(string Name, string Password) : this(Name, Password, new List<User>())
        {
        }

        private Room(string Name, string Password, List<User> users)
        {
            this.Name = Name;
            this.Password = Password;
            _users = users;
        }

        public void AddUser(User user)
        {
            if (!_users.Contains(user))
            {
                _users.Add(user);
                user.Session.Room = this;
                UserJoined?.Invoke(this, user);
            }
        }

        public bool DeleteUser(User user)
        {
            if (_users.Remove(user))
            {
                user.Session.Room = null;
                UserLeaved?.Invoke(this, user);
                return true;
            }
            return false;
        }

    }
}
