﻿using System;
using System.ComponentModel.DataAnnotations;

namespace DarlingBotNet.DataBase
{
    public class DbEntity
    {
        [Key]
        public int Id { get; set; }
        public DateTime? DateAdded { get; set; } = DateTime.UtcNow;
    }
}
