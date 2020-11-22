using System;
using System.Collections.Generic;
using System.Text;

namespace DarlingBotNet.DataBase.Database.Models
{
    public class QiwiTransactions
    {
        public ulong Id { get; set; }
        public ulong discord_id { get; set; }
        public ulong invoice_ammount { get; set; }
        public DateTime invoice_date_add { get; set; }
    }
}
