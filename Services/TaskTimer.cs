using DarlingBotNet.DataBase;
using DarlingBotNet.DataBase.Database.Models;
using DarlingBotNet.Services.Sys;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace DarlingBotNet.Services
{
    public class TaskTimer
    {
        internal static DiscordSocketClient client { get; set; }
        private static List<TaskInfo> ListTasks = new List<TaskInfo>();
        private static List<TempUser> UserTemped = new List<TempUser>();

        internal static Task StartTempMute(TempUser tu)
        {
            UserTemped.Add(tu);
            Timer TaskTime = new Timer((tu.ToTime - DateTime.Now.AddHours(3)).TotalMilliseconds);
            TaskTime.AutoReset = false;
            TaskTime.Elapsed += new ElapsedEventHandler(OnTimedTempMute);
            TaskTime.Start();

            return Task.CompletedTask;
        }
        private static async void OnTimedTempMute(Object source, ElapsedEventArgs e)
        {
            using (var DBcontext = new DBcontext())
            {
                var Temped = DBcontext.TempUser.FirstOrDefault(x => UserTemped.FirstOrDefault(z => z.ToTime == x.ToTime && z.UserId == x.UserId && z.GuildId == x.GuildId) != null);
                var GuildDB = DBcontext.Guilds.FirstOrDefault(x => x.GuildId == Temped.GuildId);
                var Guild = client.GetGuild(Temped.GuildId);
                var usr = Guild.GetUser(Temped.UserId);
                await OtherSettings.CheckRoleValid(usr, GuildDB.voicemuterole, true);
                await OtherSettings.CheckRoleValid(usr, GuildDB.chatmuterole, true);

                DBcontext.TempUser.Remove(Temped);
                await DBcontext.SaveChangesAsync();
            }
        }


        internal static Task StartTimerNow(Tasks Tasks)
        {
            ListTasks.Add(new TaskInfo() { Id = Tasks.Id, Time = Tasks.Times });
            Timer TaskTime = new Timer((Tasks.Times - DateTime.Now.AddHours(3)).TotalMilliseconds);
            TaskTime.AutoReset = Tasks.Repeat;
            TaskTime.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            TaskTime.Start();

            return Task.CompletedTask;
        }
        internal static Task StartTimer()
        {
            using (var DBcontext = new DBcontext())
            {
                var Tasks = DBcontext.Tasks.AsQueryable();
                foreach (var Task in Tasks)
                {
                    if (((Task.Times - DateTime.Now).TotalDays <= 1 && !Task.Repeat) || Task.Repeat)
                    {
                        ListTasks.Add(new TaskInfo() { Id = Task.Id, Time = Task.Times });
                        Timer TaskTime;
                        if (!Task.Repeat)
                            TaskTime = new Timer((Task.Times - DateTime.Now.AddHours(3)).TotalMilliseconds);
                        else
                        {
                            var ThisTime = DateTime.Now.AddHours(3);
                            var TimeUser = new TimeSpan(Task.Times.Hour, Task.Times.Minute, Task.Times.Second);
                            if (TimeUser.Hours < ThisTime.Hour)
                                TimeUser = new TimeSpan(Task.Times.Hour + 24, Task.Times.Minute, Task.Times.Second);
                            TimeUser = TimeUser.Subtract(new TimeSpan(ThisTime.Hour, ThisTime.Minute, ThisTime.Second));
                            TaskTime = new Timer(TimeUser.TotalMilliseconds);
                        }
                            
                        TaskTime.AutoReset = Task.Repeat;
                        TaskTime.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                        TaskTime.Start();
                    }
                }
                return Task.CompletedTask;
            }
        }

        private static async void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            using (var DBcontext = new DBcontext())
            {
                var Task = DBcontext.Tasks.AsEnumerable().FirstOrDefault(x => ListTasks.FirstOrDefault(z => z.Id == x.Id && z.Time == x.Times) != null);
                if (Task != null)
                {
                    var Guild = client.GetGuild(Task.GuildId);
                    bool DeleteTask = false;
                    if (Guild != null)
                    {
                        var Channel = Guild.GetTextChannel(Task.ChannelId);
                        if (Channel != null)
                        {
                            var Check = MessageBuilder.EmbedUserBuilder(Task.Message);
                            if (Check.Item2 == "ERROR")
                            {
                                await Channel.SendMessageAsync(Task.Message);
                            }
                            else
                            {
                                await Channel.SendMessageAsync(Check.Item2, false, Check.Item1.Build());
                            }
                        }
                        else
                            DeleteTask = true;
                    }
                    else
                        DeleteTask = true;

                    if (!Task.Repeat)
                        DeleteTask = true;

                    if (DeleteTask)
                    {
                        ListTasks.Remove(ListTasks.FirstOrDefault(x => x.Id == Task.Id && x.Time == Task.Times));
                        DBcontext.Remove(Task);
                    }
                    await DBcontext.SaveChangesAsync();
                }
            }
        }
    }

    public class TaskInfo
    {
        public ulong Id { get; set; }
        public DateTime Time { get; set; }
    }

}
