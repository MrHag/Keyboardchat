using Keyboardchat.DataBase.Models;
using Microsoft.EntityFrameworkCore;
namespace Keyboardchat.DataBase
{
    public class DatabaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(@"server=localhost;database=chat;uid=Chat;pwd=pass");
        }

    }
}
