namespace Keyboardchat.Models.Network
{
    public class UserInfo
    {
        public uint id;
        public string name;
        public string avatar;

        public UserInfo(uint id, string name, string avatar)
        {
            this.id = id;
            this.name = name;
            this.avatar = avatar;
        }
    }
}
