﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MUNityCore.Models.Conference;
using MUNityCore.Models.Conference.Roles;
using MUNityCore.Models.Organisation;

namespace MUNityCore.Services
{
    public interface IConferenceService
    {
        Project CreateProject(string name, string abbreviation, Organisation organisation);

        Task<Project> GetProject(string id);

        Task<Project> GetProjectWithConferences(string id);

        Conference CreateConference(string name, string fullname, string abbreviation, Project project);

        Task<Conference> GetConference(string id);

        Committee CreateCommittee(Conference conference, string name, string fullname, string abbreviation);

        Task<Committee> GetCommittee(string id);

        public TeamRole CreateLeaderRole(Conference conference);

        public TeamRole CreateTeamRole(Conference conference, string roleName, TeamRole parentRole = null,
            RoleAuth auth = null);

        public Participation Participate(Models.Core.User user, AbstractRole role);

        public IQueryable<AbstractRole> GetUserRolesOnConference(string username, string conferenceid);

        Task SaveDatabaseChanges();

        Task<bool> SetConferenceName(string conferenceid, string newname);

        Task<bool> IsConferenceNameTaken(string name);

        Task<bool> IsConferenceFullNameTaken(string fullname);

        Task<bool> SetConferenceFullName(string conferenceid, string newfullname);

        Task<bool> SetConferenceAbbreviation(string conferenceid, string abbreviation);

        Task<bool> SetConferenceDate(string conferenceid, DateTime startDate, DateTime endDate);
    }
}
