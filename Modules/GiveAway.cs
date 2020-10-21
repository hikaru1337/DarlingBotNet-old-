//using DarlingBotNet.DataBase;
//using DarlingBotNet.DataBase.Database.Models;
//using Discord;
//using Discord.Commands;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace DarlingBotNet.Modules
//{
//    public class GiveAway : ModuleBase<SocketCommandContext>
//    {

//        public class GiveAwayTime
//        {
//            public ulong Id { get; set; }
//            public bool End { get; set; }
//        }

//        private static List<GiveAwayTime> ListGiveAway = new List<GiveAwayTime>();

//        public async Task GiveAwayStart(short Time, ushort winner, [Remainder] string Given = null)
//        {
//            using (var DBcontext = new DBcontext())
//            {
//                var emb = new EmbedBuilder().WithColor(255,0,94).WithAuthor(":game_die: **РОЗЫГРЫШ**  :game_die:");
//                var GiveAwaysCount = DBcontext.GiveAways.Count(x => x.GuildId == Context.Guild.Id);
//                if (GiveAwaysCount > 5)
//                {
//                    if (Time < 5)
//                    {
//                        if (Time > 1440)
//                        {
//                            var TimeToIvent = DateTime.Now.AddMinutes(Time);
//                            var TimeToGo = TimeToIvent - DateTime.Now;
//                            var text = $"Розыгрыш ***{Given} ***\nНажмите на эмодзи 🎟 чтобы учавствовать!\nОсталось {(TimeToGo.TotalSeconds >= 3600 ? $"{TimeToGo.Hours} часов" : "")}  и {TimeToGo.Minutes} минут!";
//                            var ReactionDice = new Emoji("🎟");

//                            var message = await Context.Channel.SendMessageAsync("", false, emb.Build());
//                            await message.AddReactionAsync(ReactionDice);
//                            var Task = DBcontext.Add(new GiveAways() { GuildId = Context.Guild.Id, MessageId = message.Id, Times = TimeToIvent }).Entity;
//                            var ListTask = new GiveAwayTime() { Id = Task.Id, End = false };
//                            ListGiveAway.Add(ListTask);
//                            await DBcontext.SaveChangesAsync();

//                            while (TimeToIvent > DateTime.Now)
//                            {
//                                if ((TimeToIvent - DateTime.Now).Minutes % 2 == 0)
//                                {
//                                    emb.WithDescription(text);
//                                    await message.ModifyAsync(x=>x.Embed = emb.Build());
//                                }
//                                var Tasks = ListGiveAway.FirstOrDefault(x => x == ListTask);
//                                if (Tasks != null && Tasks.End)
//                                {
//                                    ListTask = Tasks;
//                                    break;
//                                }
//                            }

//                            if(ListTask.End)
//                                emb.WithDescription("Розыгрыш завершен администрацией!");
//                            else
//                            {
//                                var messageend = await message.Channel.GetMessageAsync(message.Id);

//                                var users = messageend.GetReactionUsersAsync(ReactionDice, int.MaxValue);
//                               if(users.CountAsync(x=>x.Count(x=>!x.IsBot) ) > winner)
//                            }

                            

//                        }
//                        else
//                            emb.WithDescription("Время розыгрыша не может превышать 1440 минут(1 день)!");
//                    }
//                    else
//                        emb.WithDescription("Время розыгрыша не может быть меньше 5 минут");
//                }
//                else
//                    emb.WithDescription("Кол-во одновременных розыгрышей, не может превышать 5!");
//            }
            


//            var embz = new EmbedBuilder();
//            if (1 > userz.Count)
//            {
//                rend = false;
//                bool end = false;
//                EmbedBuilder emb = new EmbedBuilder();
//                if (tgive.ToLower().Contains("m") || tgive.ToLower().Contains("s") || tgive.ToLower().Contains("h"))
//                {
//                    if (winner >= 1 && winner <= 10)
//                    {
//                        int t = Int32.Parse(tgive.Remove(tgive.Length - 1));

//                        MyEmbedBuilder.WithColor(new Color(255, 255, 0));
//                        var Name = MyEmbedField.WithName(":game_die: **РОЗЫГРЫШ**  :game_die:");
//                        MyEmbedField.WithIsInline(true);


//                        if (tgive.Contains("h"))
//                        {
//                            var msg = MyEmbedField.WithValue($"Розыгрыш ***{Given} ***\nНажмите на эмодзи 🎟 чтобы учавствовать!\nОсталось: {t} часов");
//                            t = t * 3600;
//                        }
//                        if (tgive.Contains("m"))
//                        {
//                            var msg = MyEmbedField.WithValue($"Розыгрыш ***{Given} ***\nНажмите на эмодзи 🎟 чтобы учавствовать!\nОсталось: {t} минут");
//                            t = t * 60;
//                        }
//                        if (tgive.Contains("s"))
//                        {
//                            var msg = MyEmbedField.WithValue($"Розыгрыш ***{Given} ***\nНажмите на эмодзи 🎟 чтобы учавствовать!\nОсталось: {t} секунд");
//                        }

//                        MyEmbedBuilder.AddField(MyEmbedField);
//                        var message = await Context.Channel.SendMessageAsync("", false, MyEmbedBuilder.Build());
//                        await message.AddReactionAsync(dice);
//                        RestUserMessage msgz = message;
//                        Global.MessageIdToTrack = msgz.Id;

//                        while (t > 0)
//                        {
//                            if (t >= 3600)
//                            {
//                                await Task.Delay(60000);
//                                t -= 60;
//                                int t3 = t;
//                                t3 = t3 / 3600;
//                                int time_minutes = t;
//                                time_minutes = (t / 60) % 60;
//                                MyEmbedField.WithValue($"Розыгрыш ***{Given} ***\nНажмите на эмодзи 🎟 чтобы учавствовать!\nОсталось: {t3} часов {time_minutes} минут.");

//                            }
//                            if (t >= 60 && t < 3600)
//                            {
//                                await Task.Delay(5000);
//                                t -= 5;
//                                int t2 = t;
//                                t2 = t2 / 60;
//                                int time_seconds = t % 60;
//                                MyEmbedField.WithValue($"Розыгрыш ***{Given} ***\nНажмите на эмодзи 🎟 чтобы учавствовать!\nОсталось: {t2} минут {time_seconds} секунд.");
//                            }
//                            if (t < 60)
//                            {
//                                await Task.Delay(5000);
//                                t -= 5;
//                                MyEmbedField.WithValue($"Розыгрыш ***{Given} ***\nНажмите на эмодзи 🎟 чтобы учавствовать!\nОсталось: {t} секунд");
//                            }
//                            var newMessage = await message.Channel.GetMessageAsync(message.Id) as IUserMessage;
//                            var embed2 = new EmbedBuilder();
//                            embed2.AddField(Name);
//                            embed2.WithColor(new Color(255, 255, 0));
//                            await newMessage.ModifyAsync(m => m.Embed = embed2.Build());
//                            if (rend == true) break;
//                        }

//                        await message.RemoveReactionAsync(dice, message.Author);

//                        userz.Remove(601439123152306207);
//                        var messageend = await message.Channel.GetMessageAsync(message.Id) as IUserMessage;
//                        //embedend.AddField(Name);
//                        var embedend = new EmbedBuilder();


//                        if (rend == true) embedend.WithTitle("Розыгрыш завершен администрацией!");
//                        else
//                        {
//                            if (userz.Count() > 0)
//                            {
//                                if (winner > userz.Count())
//                                {
//                                    embedend.WithTitle(":game_die: **РОЗЫГРЫШ**  :game_die:");
//                                    embedend.WithDescription($"Для розыгрыша не набрано {winner - userz.Count()} людей!").WithColor(255, 0, 94);
//                                }
//                                else
//                                {
//                                    for (int i = 0; i < winner; i++)
//                                    {
//                                        ulong winneruzr = userz.ElementAt(rand.Next(userz.Count));
//                                        var winnerz = Context.Guild.GetUser(winneruzr);
//                                        //MyEmbedField.WithValue($"***Поздравляю*** {winnerz.Mention} победил!\n Он получает приз: {Given}");
//                                        embedend.WithTitle(":game_die: **РОЗЫГРЫШ**  :game_die:");
//                                        embedend.AddField($"***Поздравляю***", $" {winnerz.Mention} победил!\nПриз: {Given}").WithColor(255, 0, 94);
//                                        userz.Remove(winneruzr);
//                                    }
//                                }

//                            }
//                            else embedend.WithTitle("В розыгрыше ни кто не участвовал!");
//                        }
//                        userz.Clear();

//                        embedend.WithColor(new Color(255, 0, 94));
//                        end = true;
//                        await messageend.ModifyAsync(m => m.Embed = embedend.Build());
//                        await messageend.AddReactionAsync(trophy);
//                    }
//                    else emb.WithTitle("ОШИБКА").WithDescription($"Пользователей должно быть не больше 10 и не меньше 1.").WithColor(255, 0, 94).WithThumbnailUrl(icon.iconget("error"));
//                }
//                else emb.WithTitle("ОШИБКА").WithDescription($"Время розыгрыша содержит некорректное время.\n Подробнее в `hika!ghelp`").WithColor(255, 0, 94).WithThumbnailUrl(icon.iconget("error"));

//                if (end != true) await Context.Channel.SendMessageAsync("", false, emb.Build());
//            }
//            else { embz.WithTitle("ОШИБКА").WithDescription($"Розыгрыш уже запущен! Дождитесь окончание прошлого розыгрыша!").WithColor(255, 0, 94).WithThumbnailUrl(icon.iconget("error")); await Context.Channel.SendMessageAsync("", false, embz.Build()); }

//        }
//    }
//}
