using System.ComponentModel.DataAnnotations;


namespace Keyboardchat.DataBase.Models
{
    public class User
    {
        [Key]
        public uint UserId { get; set; }
        public string Name { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] Avatar { get; set; }
    }
}
