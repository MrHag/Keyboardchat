using KeyBoardChat.DataBase.Models;
using Microsoft.EntityFrameworkCore;

namespace KeyBoardChat.DataBase
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
