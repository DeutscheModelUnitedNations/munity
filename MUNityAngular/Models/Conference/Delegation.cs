﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MUNityAngular.Models.Conference
{
    public class Delegation
    {
        [Column(TypeName = "varchar(80)")]
        public string DelegationId { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string Name { get; set; }

        [Column(TypeName = "varchar(250)")]
        public string FullName { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string Abbreviation { get; set; }

        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public Conference Conference { get; set; }

        public Delegation()
        {
            DelegationId = Guid.NewGuid().ToString();
        }
    }
}
