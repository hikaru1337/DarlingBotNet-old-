//using DarlingBotNet.DataBase.Database.Repositories;
//using DarlingBotNet.DataBase.Database.Repositories.Impl;
//using Microsoft.EntityFrameworkCore;
//using NadekoBot.Core.Services.Database;
//using System;
//using System.Linq;
//using System.Threading.Tasks;

//namespace DarlingBotNet.DataBase
//{
//    public sealed class UnitOfWork : IUnitOfWork
//    {
//        public DarlingContext _context { get; }

//        private IUsersRepository _Users;
//        public IUsersRepository Users => _Users ?? (_Users = new UsersRepository(_context));

//        private IGuildsRepository _Guilds;
//        public IGuildsRepository Guilds => _Guilds ?? (_Guilds = new GuildsRepository(_context));

//        private IChannelsRepository _Channels;
//        public IChannelsRepository Channels => _Channels ?? (_Channels = new ChannelsRepository(_context));

//        private ILVLROLESRepository _LVLROLES;
//        public ILVLROLESRepository LVLROLES => _LVLROLES ?? (_LVLROLES = new LVLROLESRepository(_context));

//        private IEmoteClickRepository _EmoteClick;
//        public IEmoteClickRepository EmoteClick => _EmoteClick ?? (_EmoteClick = new EmoteClickRepository(_context));

//        private IPrivateChannelsRepository _PrivateChannels;
//        public IPrivateChannelsRepository PrivateChannels => _PrivateChannels ?? (_PrivateChannels = new PrivateChannelsRepository(_context));

//        private IClansRepository _Clans;
//        public IClansRepository Clans => _Clans ?? (_Clans = new ClansRepository(_context));

//        private IWarnsRepository _Warns;
//        public IWarnsRepository Warns => _Warns ?? (_Warns = new WarnsRepository(_context));

//        private ITempUserRepository _TempUser;
//        public ITempUserRepository TempUser => _TempUser ?? (_TempUser = new TempUserRepository(_context));

//        public UnitOfWork(DarlingContext context)
//        {
//            _context = context;
//        }

//        public int SaveChanges() => _context.SaveChanges(true);

//        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync(true);

//        public void Dispose()
//        {
//            _context.Dispose();
//            GC.SuppressFinalize(this);
//        }
//    }
//}
