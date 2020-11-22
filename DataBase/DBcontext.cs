using DarlingBotNet.DataBase.Database.Crypto;
using DarlingBotNet.DataBase.Database.Models;
using DarlingBotNet.Modules;
using DarlingBotNet.Services;
using Microsoft.EntityFrameworkCore;

namespace DarlingBotNet.DataBase
{
    public class DBcontext : DbContext
    {
        public DbSet<Users> Users { get; set; }
        public DbSet<Guilds> Guilds { get; set; }
        public DbSet<Channels> Channels { get; set; }
        public DbSet<LVLROLES> LVLROLES { get; set; }
        public DbSet<RoleSwaps> RoleSwaps { get; set; }
        public DbSet<EmoteClick> EmoteClick { get; set; }
        public DbSet<PrivateChannels> PrivateChannels { get; set; }
        public DbSet<Clans> Clans { get; set; }
        public DbSet<Warns> Warns { get; set; }
        public DbSet<Tasks> Tasks { get; set; }
        public DbSet<TempUser> TempUser { get; set; }
        public DbSet<QiwiTransactions> QiwiTransaction { get; set; }
        public DbSet<DarlingBoost> DarlingBoost { get; set; }
        //public DbSet<Lottery> Lottery { get; set; }
        //public DbSet<Crypto_Items> Crypto_Items { get; set; }
        //public DbSet<Crypto_Profile> Crypto_Profile { get; set; }
        //public DbSet<Crypto_Study> Crypto_Study { get; set; }
        //public DbSet<Crypto_Studys> Crypto_Studys { get; set; }
        //public DbSet<Crypto_Valute> Crypto_Valute { get; set; }
        //public DbSet<Crypto_Work> Crypto_Work { get; set; }

        //public DbSet<GiveAways> GiveAways { get; set; }
        //public DbSet<RussiaGame_Profile> RG_Profile { get; set; }
        //public DbSet<RussiaGame_Item> RG_Item { get; set; }
        //public DbSet<RussiaGame_Study> RG_Study { get; set; }
        //public DbSet<RussiaGame_Studys> RG_Studys { get; set; }
        //public DbSet<RussiaGame_Work> RG_Work { get; set; }

        //public void ApplicationContext() => Database.EnsureCreated();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            //optionsBuilder.UseSqlite(new SqliteConnectionStringBuilder(BotSettings.ConnectionString).ToString());
            optionsBuilder.UseSqlite(BotSettings.ConnectionString);
            //optionsBuilder.UseSqlServer("Server=DESKTOP-MRVJEIO\\SQLEXPRESS;Database=DarlingBotCore;Trusted_Connection=True;");

        }
    }

}
