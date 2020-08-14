using DarlingBotNet.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace DarlingBotNet.DataBase
{
    public class DarlingContextFactory : IDesignTimeDbContextFactory<DarlingContext>
    {
        public DarlingContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DarlingContext>();
            optionsBuilder.UseSqlite(new SqliteConnectionStringBuilder(BotSettings.ConnectionString).ToString());
            var ctx = new DarlingContext(optionsBuilder.Options);
            ctx.Database.SetCommandTimeout(60);
            return ctx;
        }
    }

    public class DarlingContext : DbContext
    {
        public DbSet<Users> Users { get; set; }
        public DbSet<Guilds> Guilds { get; set; }
        public DbSet<Channels> Channels { get; set; }
        public DbSet<LVLROLES> LVLROLES { get; set; }
        public DbSet<EmoteClick> EmoteClick { get; set; }
        public DbSet<PrivateChannels> PrivateChannels { get; set; }
        public DbSet<Clans> Clans { get; set; }
        public DbSet<Warns> Warns { get; set; }
        public DbSet<TempUser> TempUser { get; set; }

        public DarlingContext(DbContextOptions<DarlingContext> options) : base(options)
        {
        }
    }
}
