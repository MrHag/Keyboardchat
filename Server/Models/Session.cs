using System.Collections.Generic;

namespace KeyBoardChat.Models
{
    public class Session
    {
        private List<Connection> _connections;

        public IEnumerable<Connection> Connections { get { return _connections; } }

        private User _user;
        public User User
        {
            get { return _user; }
            set
            {
                var input = value;
                UserChanged?.Invoke(this, input);

                if (input != null)
                    input.Session = this;

                if (_user != null)
                    _user.Session = null;

                _user = input;
            }
        }

        private Room _room;
        public Room Room
        {
            get { return _room; }
            set
            {
                var input = value;

                if (_room != null)
                    _room.DeleteUser(User);

                RoomChanged?.Invoke(this, input);

                if (input != null)
                    input.AddUser(_user);

                _room = input;
            }
        }

        public event DataDelegate<User> UserChanged;

        public event DataDelegate<Room> RoomChanged;

        public event DataDelegate<Connection> OnConnectionAdded;

        public event DataDelegate<Connection> OnConnectionRemoved;

        public Session()
        {
            _connections = new List<Connection>();
        }

        public void AddConnection(Connection connection)
        {
            _connections.Add(connection);
            connection.Session = this;

            OnConnectionAdded?.Invoke(this, connection);
        }

        public bool RemoveConnection(Connection connection)
        {

            if (_connections.Remove(connection))
            {
                connection.Session = null;
                OnConnectionRemoved?.Invoke(this, connection);
                return true;
            }
            return false;
        }

        public void ClearConnections()
        {
            foreach (var connection in Connections)
                RemoveConnection(connection);
        }

    }
}
