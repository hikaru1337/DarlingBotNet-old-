using System;
using System.Collections.Generic;
using System.Text;

namespace DarlingBotNet.DataBase.Database.Models
{
    public class Pet
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public ulong UserId { get; set; }
        public PetTypes PetType { get; set; }
        public DateTime DateOfBirth { get; set; }
        public sbyte HP { get; set; }
        public sbyte MOOD { get; set; }
        public sbyte EAT { get; set; }
        public sbyte SLEEP { get; set; }
        public DateTime LastEat { get; set; }
        public DateTime LastStartSleep { get; set; }
        public DateTime LastEndSleep { get; set; }


        public enum PetTypes
        {
            Кот,
            Собака,
            Попугай,
            Крыса,
            Хомяк,
            Кролик,
            Морская_свинка,
            РобоПёс
        }
    }
}
