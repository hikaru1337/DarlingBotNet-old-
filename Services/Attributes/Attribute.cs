using DarlingBotNet.DataBase;

using Discord.Commands;
using Newtonsoft.Json;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DarlingBotNet.Services;
using Microsoft.Extensions.DependencyInjection;
using DarlingBotNet.Modules;
using Discord.WebSocket;

namespace DarlingBotNet.Services
{

    public class DescriptionsAttribute : SummaryAttribute
    {
        public DescriptionsAttribute([CallerMemberName] string memberName = "") : base(Initiliaze.Load(memberName).Desc) { }
    }

    public class PermissionBlockCommand : PreconditionAttribute
    {

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {

            using (var Xontext = new DBcontext())
            {
                var glds = Xontext.Guilds.AsNoTracking().FirstOrDefault(x => x.guildid == context.Guild.Id);
                bool es = false;
                if (command.Aliases.Count == 1)
                {
                    if (glds.CommandInviseList.FirstOrDefault(x => x == command.Aliases[0].ToLower()) != null)
                        es = true;
                }
                else if (command.Aliases.Count == 2)
                {
                    if (glds.CommandInviseList.FirstOrDefault(x => x == command.Aliases[0].ToLower() || x == command.Aliases[1].ToLower()) != null)
                        es = true;
                }

                if (es)
                    return Task.FromResult(PreconditionResult.FromError($"Команда отключена создателем сервера."));
                else
                    return Task.FromResult(PreconditionResult.FromSuccess());

            }
        }
    }

    public class PermissionServerOwner : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.User.Id == BotSettings.hikaruid)
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
            {
                if (context.Guild.OwnerId != context.User.Id)
                    return Task.FromResult(PreconditionResult.FromError($"Вы не являетесь создателем сервера чтобы использовать эту команду"));
                else
                    return Task.FromResult(PreconditionResult.FromSuccess());
            }
        }
    }

    public class PermissionViolation : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            using (var Xontext = new DBcontext())
            {
                var Guild = Xontext.Guilds.AsNoTracking().FirstOrDefault(x => x.guildid == context.Guild.Id);
                if (Guild.ViolationSystem != 2)
                    return Task.FromResult(PreconditionResult.FromError($"У вас не выбрана warn система!\n{Guild.Prefix}vs"));
                else
                    return Task.FromResult(PreconditionResult.FromSuccess());
            }

        }
    }

    public class PermissionRussiaGame : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            using (var DBcontext = new DBcontext())
            {
                var usr = DBcontext.RG_Profile.AsNoTracking().FirstOrDefault(x => x.userid == context.User.Id && x.guildid == context.Guild.Id);
                if (usr == null) usr = RussiaGame.CreateUser(context.User as SocketUser).Result;
                if (command.Name == "rgstudys" && usr.StudyNowid == 0)
                    return Task.FromResult(PreconditionResult.FromError($"У вас нет активного учебного заведения!"));

                else if (command.Name == "rgworking" && usr.workid == 0)
                    return Task.FromResult(PreconditionResult.FromError($"У вас нет работы!"));





                return Task.FromResult(PreconditionResult.FromSuccess());
            }

        }
    }

    public class PermissionClan : PreconditionAttribute
    {

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {

            using (var Xontext = new DBcontext())
            {
                var usr = Xontext.Users.AsNoTracking().FirstOrDefault(x=>x.guildId == context.Guild.Id && x.userid == context.User.Id);
                var myclan = Xontext.Clans.AsNoTracking().FirstOrDefault(x=>x.guildId ==context.Guild.Id && x.OwnerId == context.User.Id);
                if ((command.Name == "clandelete" || command.Name == "clanperm" || command.Name == "clanownertake") && myclan == null)
                    return Task.FromResult(PreconditionResult.FromError($"У вас нет своего клана!"));
                else if (command.Name == "clanclaims" || command.Name == "clankick")
                {
                    if (myclan == null && usr.clanInfo != Users.UserClanRole.moder)
                        return Task.FromResult(PreconditionResult.FromError($"У вас нет своего клана или вы не являетесь модератором в который вступили!"));
                }
                else if (command.Name == "claninfo")
                {
                    if(myclan == null && (usr.clanInfo == Users.UserClanRole.wait || usr.clanInfo == Users.UserClanRole.ready))
                        return Task.FromResult(PreconditionResult.FromError($"У вас нет своего клана или вы не вступили чтобы посмотреть информацию!"));
                }
                else if(command.Name == "clanleave")
                {
                    if(usr.clanInfo == Users.UserClanRole.wait || usr.clanInfo == Users.UserClanRole.ready)
                        return Task.FromResult(PreconditionResult.FromError($"Вы не вступили в клан, чтобы выходить откуда либо!"));
                }
                else if(command.Name == "clancreate" && myclan != null)
                    return Task.FromResult(PreconditionResult.FromError($"Для того чтобы создать новый клан, удалите или передайте существующий!"));

                return Task.FromResult(PreconditionResult.FromSuccess());
            }
        }
    }

    public class CommandsAttribute : CommandAttribute
    {
        public CommandsAttribute([CallerMemberName] string memberName = "") : base(Initiliaze.Load(memberName.ToLowerInvariant()).Cmd) { }
    }

    public class UsageAttribute : RemarksAttribute
    {
        public UsageAttribute([CallerMemberName] string memberName = "") : base(GetUsage(memberName)) { }

        public static string GetUsage(string memberName)
        {
            var usage = Initiliaze.Load(memberName.ToLowerInvariant()).Usage;
            return JsonConvert.SerializeObject(usage);
        }
    }

    public class AliasesAttribute : AliasAttribute
    {
        public AliasesAttribute([CallerMemberName] string memberName = "") : base(Initiliaze.Load(memberName.ToLowerInvariant()).Usage.Skip(1).ToArray()) { }
    }

    public class Initiliaze
    {
        private static readonly Dictionary<string, Commands> _commandData;
        static Initiliaze()
        {
            _commandData = JsonConvert.DeserializeObject<Dictionary<string, Commands>>(File.ReadAllText("CommandsList.json"));
        }
        public static Commands Load(string key)
        {
            _commandData.TryGetValue(key, out var toReturn);

            if (toReturn == null)
                return new Commands
                {
                    Cmd = key,
                    Desc = key,
                    Usage = new[] { key }
                };

            return toReturn;
        }

        public class Commands
        {
            public string Cmd { get; set; }
            public string Desc { get; set; }
            public string[] Usage { get; set; }
        }
    }
}
