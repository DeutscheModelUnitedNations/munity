﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MUNityCore.Models.Conference
{

    /// <summary>
    /// Use this if you want a group of users make custom applications but every one of this applications
    /// should be collected in one giant stack and looked at together in one big bundle.
    /// If you want multiple users to apply to the same Role or Delegation you can also use
    /// GroupApplication.
    /// <see cref="GroupApplication"/>
    ///
    /// <seealso cref="RoleApplication"/>
    /// </summary>
    [DataContract]
    public class GroupedRoleApplication
    {
        [DataMember]
        public int GroupedRoleApplicationId { get; set; }

        [DataMember]
        public List<RoleApplication> Applications { get; set; }

        [DataMember]
        public DateTime CreateTime { get; set; }

        [Timestamp]
        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        [IgnoreDataMember]
        public byte[] GroupedRoleApplicationTimestamp { get; set; }
    }
}
