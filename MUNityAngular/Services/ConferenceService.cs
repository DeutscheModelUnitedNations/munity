﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MUNityAngular.DataHandlers.Database;
using MySql.Data.MySqlClient;
using MUNityAngular.Models.User;
using MUNityAngular.DataHandlers.EntityFramework;
using System.Text.RegularExpressions;
using MUNityAngular.DataHandlers.EntityFramework.Models;
using Microsoft.EntityFrameworkCore;
using MUNityAngular.Exceptions.ConferenceExceptions;
using MUNityAngular.Models.Conference;
using MUNityAngular.Models.Conference.Roles;
using MUNityAngular.Models.Organisation;
using MUNityAngular.Util.Extenstions;

namespace MUNityAngular.Services
{
    public class ConferenceService : IConferenceService
    {
        private MunCoreContext _context;

        public Project CreateProject(string name, string abbreviation, Organisation organisation)
        {
            var project = new Project
            {
                ProjectName = name,
                ProjectAbbreviation = abbreviation,
                ProjectOrganisation = organisation
            };

            if (!_context.Projects.Any(n => n.ProjectId == abbreviation))
                project.ProjectId = abbreviation;

            _context.Projects.Add(project);
            _context.SaveChanges();
            return project;
        }

        public Task<Project> GetProject(string id)
        {
            return _context.Projects.FirstOrDefaultAsync(n => n.ProjectId == id);
        }

        /// <summary>
        /// Creates an empty conference. This conference does not contain any roles
        /// participations etc.
        /// </summary>
        /// <param name="name">The name of the conference like MUN Schleswig-Holstein 2021</param>
        /// <param name="fullname">The full name of the conference for example Model United Nations Schleswig-Holstein 2021</param>
        /// <param name="abbreviation">A short name of the conference like: MUN-SH 2021</param>
        /// <param name="project">The project that this conference belongs to. Like the Model United Nations Schleswig-Holstein projects.</param>
        /// <returns></returns>
        public Conference CreateConference(string name, string fullname, string abbreviation, Project project)
        {
            if (_context.Conferences.Any(n => n.Name == name || n.FullName == fullname))
                throw new NameAlreadyTakenException("The conferencename or Fullname is already taken by another conference.");

            if (project == null)
                throw new ArgumentException("The project cannot be null!");

            var conference = new Conference();
            conference.Name = name;
            conference.FullName = fullname;
            conference.Abbreviation = abbreviation;
            if (!_context.Conferences.Any(n => n.ConferenceId == abbreviation))
                conference.ConferenceId = abbreviation;
            conference.ConferenceProject = project;

            _context.Conferences.Add(conference);
            _context.SaveChanges();

            return conference;
        }

        /// <summary>
        /// Gets a conference with the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<Conference> GetConference(string id)
        {
            return _context.Conferences.Include(n => n.Committees).FirstOrDefaultAsync(n => n.ConferenceId == id);
        }

        /// <summary>
        /// Creates a Committee for a conference.
        /// </summary>
        /// <param name="conference"></param>
        /// <param name="name"></param>
        /// <param name="fullname"></param>
        /// <param name="abbreviation"></param>
        /// <returns></returns>
        public Committee CreateCommittee(Conference conference, string name, string fullname, string abbreviation)
        {
            if (conference == null)
                throw new ArgumentException("The conference can not be null, make sure you give a valid conference when creating a committee.");

            var committee = new Committee();
            committee.Name = name;
            committee.FullName = fullname;
            committee.Abbreviation = abbreviation;

            

            string customid = conference.ConferenceId + "-" + abbreviation.ToUrlValid();
            if (!_context.Committees.Any(n => n.CommitteeId == customid))
                committee.CommitteeId = customid;


            committee.Conference = conference;
            _context.Committees.Add(committee);

            _context.SaveChanges();

            return committee;

        }

        public Task<Committee> GetCommittee(string id)
        {
            return _context.Committees.FirstOrDefaultAsync(n => n.CommitteeId == id);
        }

        public TeamRole CreateLeaderRole(Conference conference)
        {
            var roleAuth = new RoleAuth
            {
                RoleAuthName = "Leader",
                Conference = conference,
                CanEditConferenceSettings = true,
                CanSeeApplications = true,
                CanEditParticipations = true,
                PowerLevel = 1
            };
            _context.RoleAuths.Add(roleAuth);
            var role = new TeamRole
            {
                Conference = conference,
                RoleAuth = roleAuth,
                RoleName = "Leader",
                ApplicationState = EApplicationStates.Closed,
                ApplicationValue = "",
                IconName = "default",
                ParentTeamRole = null,
                TeamRoleGroup = 1
            };
            _context.TeamRoles.Add(role);
            _context.SaveChanges();
            return role;
        }

        public TeamRole CreateTeamRole(Conference conference, string roleName, TeamRole parentRole = null, RoleAuth auth = null)
        {
            var role = new TeamRole();
            role.Conference = conference;
            role.RoleName = roleName;
            role.RoleAuth = auth;

            if (parentRole != null)
                role.ParentTeamRole = parentRole;

            _context.TeamRoles.Add(role);
            _context.SaveChanges();
            return role;
        }

        public SecretaryGeneralRole CreateSecretaryGeneralRole(Conference conference, string roleName, string title, RoleAuth auth = null)
        {
            var role = new SecretaryGeneralRole();
            role.Title = title;
            role.RoleName = roleName;
            role.Conference = conference;

            _context.SecretaryGenerals.Add(role);
            _context.SaveChanges();
            return role;
        }

        public IQueryable<TeamRole> GetTeamRoles(string conferenceId)
        {
            return _context.TeamRoles.Where(n => n.Conference.ConferenceId == conferenceId);
        }

        public Delegation CreateDelegation(Conference conference, string name)
        {
            var delegation = new Delegation();
            delegation.Conference = conference;
            delegation.Name = name;

            _context.Delegation.Add(delegation);
            _context.SaveChanges();

            return delegation;
        }

        public IQueryable<Delegation> GetDelegations(string conferenceId)
        {
            return _context.Delegation.Where(c => c.Conference.ConferenceId == conferenceId);
        }

        public DelegateRole CreateDelegateRole(Committee committee, Delegation delegation, string name, bool isLeader = false)
        {
            var role = new DelegateRole();
            role.RoleName = name;
            role.Committee = committee;
            role.Conference = committee.Conference;
            role.Delegation = delegation;
            role.IsDelegationLeader = isLeader;
            role.Title = name;

            _context.Delegates.Add(role);
            _context.SaveChanges();
            return role;
        }

        public IQueryable<DelegateRole> GetDelegateRolesOfConference(string conferenceId)
        {
            return _context.Delegates.Where(c => c.Conference.ConferenceId == conferenceId);
        }

        public IQueryable<DelegateRole> GetDelegateRolesOfCommittee(string committeeId)
        {
            return _context.Delegates.Where(n => n.Committee.CommitteeId == committeeId);
        }

        public IQueryable<DelegateRole> GetDelegateRolesOfDelegation(string delegationId)
        {

            return _context.Delegates.Where(n => n.Delegation.DelegationId == delegationId);
        }

        public NgoRole CreateNgoRole(Conference conference,string roleName, string ngoName)
        {
            var role = new NgoRole();
            role.RoleName = roleName;
            role.NgoName = ngoName;
            role.Conference = conference;

            _context.NgoRoles.Add(role);
            _context.SaveChanges();
            return role;

        }

        public IQueryable<NgoRole> GetNgoRoles(string conferenceId)
        {
            return _context.NgoRoles.Where(n => n.Conference.ConferenceId == conferenceId);
        }

        public PressRole CreatePressRole(Conference conference, PressRole.EPressCategories category, string roleName)
        {
            var role = new PressRole();
            role.RoleName = roleName;
            role.Conference = conference;
            role.PressCategory = category;

            _context.PressRoles.Add(role);
            _context.SaveChanges();

            return role;
        }

        public IQueryable<PressRole> GetPressRoles(string conferenceId)
        {
            return _context.PressRoles.Where(n => n.Conference.ConferenceId == conferenceId);
        }

        public Participation Participate(Models.Core.User user, AbstractRole role)
        {
            var participation = new Participation();
            participation.User = user;
            participation.Role = role;

            _context.Participations.Add(participation);
            _context.SaveChanges();

            return participation;
        }

        public IQueryable<Participation> GetUserParticipations(Models.Core.User user)
        {
            return _context.Participations.Where(n => n.User == user);
        }

        public IQueryable<Participation> GetConferenceParticipations(string conferenceId)
        {
            var list = _context.Participations.Where(n => n.Role.Conference.ConferenceId == conferenceId);
            return list;
        }

        public ConferenceService(MunCoreContext context)
        {
            this._context = context;
            Console.WriteLine("Conference-Service Started!");
        }
    }
}
