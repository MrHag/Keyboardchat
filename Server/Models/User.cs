namespace KeyBoardChat.Models
{

    public class User
    {
        public Session Session { get; set; }
        public uint UID { get; private set; }

        private string _name;
        public string Name
        {
            get
            { return _name; }

            set
            {
                var input = value;

                if (_name == input)
                    return;

                _name = input;
                OnNameChanged?.Invoke(this, _name);
            }
        }

        public event DataDelegate<string> OnNameChanged;

        public User(uint id, string Name)
        {
            UID = id;
            _name = Name;
        }

    }
}
