﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MUNityCore.Schema.Request
{
    public class AddTeamMemberRequestBody
    {
        public string Username { get; set; }

        public string RoleId { get; set; }
    }
}
