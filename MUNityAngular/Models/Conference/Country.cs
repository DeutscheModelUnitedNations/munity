﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MUNityAngular.Models.Conference
{
    public class Country
    {
        public enum EContinent
        {
            Africa,
            Antarctica,
            Asia,
            Australia,
            Europe,
            NorthAmerica,
            SouthAmerica
        }

        public int CountryId { get; set; }

        public EContinent Continent { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string Name { get; set; }

        [Column(TypeName = "varchar(250)")]
        public string FullName { get; set; }

        [Column(TypeName = "varchar(3)")]
        public string Iso { get; set; }

        [Timestamp]
        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public byte[] CountryTimestamp { get; set; }
    }
}
