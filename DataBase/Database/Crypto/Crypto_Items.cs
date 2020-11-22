using System;
using System.Collections.Generic;
using System.Text;

namespace DarlingBotNet.DataBase.Database.Crypto
{
    public class Crypto_Items
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public ulong UserId { get; set; }
        public ulong DefaultPrice 
        { 
            get
            {
                return CountTraded / Price;
            }
        }
        public ulong Price { get; set; }
        public ulong CountTraded { get; set; }
        public TypeItems TypeItem { get; set; }
        public TypePositions TypePosition { get;set; }

        public enum TypeItems
        {
            MineMachine,
            AntiHack,
            BotNet,
            Stiller,
            AntiVirus,
            HackSwap
        }

        public enum TypePositions
        {
            BotItem,
            Traded,
            Inventory
        }
    }
}
