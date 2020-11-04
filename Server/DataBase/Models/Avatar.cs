using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KeyBoardChat.DataBase.Models
{
    public class Avatar
    {
        [Key]
        public uint Id { get; set; }
        public byte[] AvatarData { get; set; }
    }
}
