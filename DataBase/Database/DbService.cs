//using DarlingBotNet.Services;
//using Microsoft.Data.Sqlite;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using NadekoBot.Core.Services.Database;
//using System;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;

//namespace DarlingBotNet.DataBase.Database
//{
//    public class DbService
//    {
//        private readonly DbContextOptions<DarlingContext> options;
//        //private readonly DbContextOptions<DarlingContext> migrateOptions;

//        public DbService()
//        {
//            var builder = new SqliteConnectionStringBuilder(BotSettings.ConnectionString);
//            builder.DataSource = Path.Combine(AppContext.BaseDirectory, builder.DataSource);

//            var optionsBuilder = new DbContextOptionsBuilder<DarlingContext>();
//            optionsBuilder.UseSqlite(BotSettings.ConnectionString);
//            //optionsBuilder.Options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll)
//            options = optionsBuilder.Options;

//            //optionsBuilder = new DbContextOptionsBuilder<DarlingContext>();
//            //optionsBuilder.UseSqlite(BotSettings.ConnectionString);
//            //migrateOptions = optionsBuilder.Options;
//        }

//        public void Setup()
//        {
//            using (var context = new DarlingContext(options))
//            {
//                if (context.Database.GetPendingMigrations().Any())
//                {
//                    using (var mContext = new DarlingContext(options/*migrateOptions*/))
//                    {
//                        mContext.Database.Migrate();
//                        mContext.SaveChanges();
//                        mContext.Dispose();
//                    }
//                }
//                context.SaveChanges();
//            }
//        }

//        private DarlingContext GetDbContextInternal()
//        {
//            var context = new DarlingContext(options);
//            //context.Database.SetCommandTimeout(60);
//            return context;
//        }

//        public IUnitOfWork GetDbContext() => new UnitOfWork(GetDbContextInternal());
//    }
//}