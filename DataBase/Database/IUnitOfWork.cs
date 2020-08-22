//using DarlingBotNet.DataBase;
//using DarlingBotNet.DataBase.Database.Repositories;
//using Discord;
//using System;
//using System.Threading.Tasks;

//namespace NadekoBot.Core.Services.Database
//{
//    public interface IUnitOfWork : IDisposable
//    {
//        DarlingContext _context { get; }

//        IUsersRepository Users { get; }
//        IGuildsRepository Guilds { get; }
//        IChannelsRepository Channels { get; }
//        ILVLROLESRepository LVLROLES { get; }
//        IEmoteClickRepository EmoteClick { get; }
//        IPrivateChannelsRepository PrivateChannels { get; }
//        IClansRepository Clans { get; }
//        IWarnsRepository Warns { get; }
//        ITempUserRepository TempUser { get; }

//        int SaveChanges();
//        Task<int> SaveChangesAsync();

//    }
//}
