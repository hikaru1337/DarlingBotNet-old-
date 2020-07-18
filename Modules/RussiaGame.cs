//using DarlingBotNet.DataBase;
//using DarlingBotNet.DataBase.RussiaGame;
//using DarlingBotNet.Services;
//using Discord;
//using Discord.Commands;
//using Discord.WebSocket;
//using System;
//using System.Linq;
//using System.Threading.Tasks;

//namespace DarlingBotNet.Modules
//{
//    public class RussiaGame : ModuleBase<SocketCommandContext>
//    {
//        [Aliases, Commands, Usage, Descriptions, PermissionBlockCommand]
//        public async Task RGstudy(ulong page,ulong studyid)
//        {
//            var emb = new EmbedBuilder().WithColor(255,0,94).WithAuthor("Выбор учебы");
//            var studys = new EEF<RussiaGame_Study>(new DBcontext()).Get();
//            if (page == 0 && studyid == 0)
//            {
//                if (studys.Count() == 0) emb.WithDescription("Учебных заведений нет!");
//                else
//                {
//                    if (page > Math.Ceiling(Convert.ToDouble(studys.Count()) / 5)) emb.WithDescription("404 Not Found. Вы зашли в темный район, тут никого нет.")
//                            .WithFooter($"Страница { (page == 0 ? 1 : page)}/{ Math.Ceiling(Convert.ToDouble(studys.Count()) / 5)}");
//                    else
//                    {
//                        var glds = new EEF<Guilds>(new DBcontext()).GetF(x=>x.guildId == Context.Guild.Id);
//                        emb.WithFooter($"Учиться - {glds.Prefix}rgs [page] [studyid]\nСтраница {(page == 0 ? 1 : page)}/{Math.Ceiling(Convert.ToDouble(studys.Count()) / 5)}");
//                        int circl = 0;
//                        foreach (var us in studys.Skip(Convert.ToInt32(page > 0 ? --page : page) * 5))
//                        {
//                            circl++;
//                            emb.AddField($"{us.studyid}.{us.studyName}", $"Цена: {us.StudyMoney} Время обучения: {us.DayStudying}", true);
//                            if (circl >= 5) break;
//                        }
//                    }
//                }
//            }
//            else
//            {

//            }
//            await Context.Channel.SendMessageAsync("",false,emb.Build());
//        }
//    }
//}

