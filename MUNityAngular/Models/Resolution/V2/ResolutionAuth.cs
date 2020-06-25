﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.IO;
using MongoDB.Driver.Core.Operations;

namespace MUNityAngular.Models.Resolution.V2
{

    /// <summary>
    /// A ResolutionAuth saves a resolution inside the Database and manages
    /// the access to this element. Note that this Logic is seperated from the
    /// MUN-Core this is due to a discussion that resulted in a clear split
    /// between basic MUN functions and advanced editors.
    /// </summary>
    public class ResolutionAuth
    {
        public string ResolutionId { get; set; }

        public int CreationUserId { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastChangeTime { get; set; }

        public List<ResolutionUser> AllowedEditors { get; set; }

        public List<ResolutionUser> AllowedSpectators { get; set; }

        public bool AllowPublicRead { get; set; }

        public bool AllowPublicEdit { get; set; }

        public bool AllowConferenceRead { get; set; }

        public bool AllowCommitteeRead { get; set; }

        public string PublicShortKey { get; set; }


        public string ConferenceId { get; set; }

        public string CommitteeId { get; set; }


        public ResolutionAuth(ResolutionV2 resolution)
        {
            ResolutionId = resolution.ResolutionId;
            CreationDate = DateTime.Now;
            LastChangeTime = DateTime.Now;
        }

        public ResolutionAuth()
        {
            
        }
    }
}
