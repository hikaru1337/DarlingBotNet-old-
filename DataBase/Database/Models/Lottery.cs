//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.Linq;
//using System.Text;

//namespace DarlingBotNet.DataBase.Database.Models
//{
//    public class Lottery
//    {
//        public ulong Id { get;set; }
//        public string NameLottery { get; set; }
//        public ulong MoneyWins { get; set; }
//        public uint CountWins { get; set; }
//        public int CountLots { get; set; }
//        public bool Repeat { get; set; }
//        public DateTime TimeStart { get; set; }
//        public DateTime TimeEnd { get; set; }
//        public string Lots { get; set; }
//        public ulong WinId { get; set; }

//        [NotMapped]
//        public List<ulong> LotsList
//        {
//            get
//            {
//                if (Lots != null)
//                {
//                    return Lots.Split(',').Select(ulong.Parse).ToList();
//                }
//                return null;
//            }
//            set
//            {
//                Lots = string.Join(",", value);
//            }
//        }

//        [NotMapped]
//        public ulong[,] LotsMassive
//        {
//            get
//            {
//                if (Lots != null)
//                {
//                    int x = 10;
//                    int y = 6;
//                    switch (CountLots)
//                    {
//                        case 120:
//                            x = 20;
//                            break;
//                        case 400:
//                            x = 40;
//                            y = 10;
//                            break;
//                    }
//                    var MassiveLots = Lots.Split(',');
//                    int zxc = 0;
//                    ulong[,] Mas = new ulong[y,x];
//                    for (int i = 0; i < y; i++)
//                    {
//                        for (int j = 0; j < x; j++) 
//                        {
//                            if (zxc < MassiveLots.Length)
//                            {
//                                Mas[i, j] = Convert.ToUInt64(MassiveLots[zxc]);
//                                zxc++;
//                            }
//                        }
//                    }
//                    return Mas;
//                }
//                return null;
//            }
//            set
//            {
//                Lots = string.Join(",", value);
//            }
//        }

//    }
//}
