using DarlingBotNet.DataBase;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

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
                var glds = Xontext.Guilds.First(x => x.guildId == context.Guild.Id);
                if (glds.CommandInviseList.FirstOrDefault(x => x == command.Aliases[0].ToLower() || x == command.Aliases[1].ToLower()) != null)
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
            var Guild = new EEF<Guilds>(new DBcontext()).GetF(x=>x.guildId == context.Guild.Id);
                if (Guild.ViolationSystem != 2)
                    return Task.FromResult(PreconditionResult.FromError($"У вас не выбрана warn система!\n{Guild.Prefix}.vs"));
                else
                    return Task.FromResult(PreconditionResult.FromSuccess());
            
        }
    }

    public class CommandsAttribute : CommandAttribute
    {
        public CommandsAttribute([CallerMemberName] string memberName = "") : base(Initiliaze.Load(memberName.ToLowerInvariant()).Cmd) { }
    }

    public class UsageAttribute : RemarksAttribute
    {
        public UsageAttribute([CallerMemberName] string memberName = "") : base(GetUsage(memberName)){}

        public static string GetUsage(string memberName)
        {
            var usage = Initiliaze.Load(memberName.ToLowerInvariant()).Usage;
            return JsonConvert.SerializeObject(usage);
        }
    }

    public class AliasesAttribute : AliasAttribute
    {
        public AliasesAttribute([CallerMemberName] string memberName = "") : base(Initiliaze.Load(memberName.ToLowerInvariant()).Usage.Skip(1).ToArray()) {}
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
