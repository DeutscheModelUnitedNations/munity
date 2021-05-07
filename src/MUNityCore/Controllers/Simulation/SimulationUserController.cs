﻿using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MUNity.Hubs;
using MUNity.Schema.Simulation;
using MUNityCore.Extensions.CastExtensions;
using MUNityCore.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Text;

namespace MUNityCore.Controllers.Simulation
{
    /// <summary>
    /// Handles users that are inside a slot, or slots itself
    /// </summary>
    [Route("api/Simulation/User")]
    [ApiController]
    public class SimulationUserController : ControllerBase
    {
        public IHubContext<SimulationHub, ITypedSimulationHub> HubContext { get; set; }

        private Services.SimulationService _simulationService;

        public SimulationUserController(IHubContext<SimulationHub, ITypedSimulationHub> hubContext, Services.SimulationService simulationService)
        {
            this.HubContext = hubContext;
            this._simulationService = simulationService;
        }

        /// <summary>
        /// Will create a new Slot for a user to join a simulation.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("[action]")]
        public ActionResult<SimulationUserAdminDto> CreateUser([FromBody]SimulationRequest body)
        {
            var isAllowed = this._simulationService.IsTokenValidAndUserChairOrOwner(body.SimulationId, body.Token);
            if (!isAllowed) 
                return Forbid();
            var newUser = this._simulationService.CreateUser(body.SimulationId, "");
            if (newUser == null)
                return NotFound();
            return Ok(newUser.ToSimulationUserAdminDto());
        }

        [HttpGet]
        [Route("[action]")]
        [Authorize]
        public IActionResult SimulationUserCsv(int simulationId)
        {
            var simulation = this._simulationService.GetSimulation(simulationId);
            if (simulation == null)
                return NotFound();


            string text = this._simulationService.GetSimulationUserCsv(simulationId, "https://" + Request.Host.ToUriComponent() + "/invite/");
            return File(Encoding.UTF8.GetBytes(text), "application/octet-stream", $"{simulation.Name}_Benutzer.csv");
        }


        /// <summary>
        /// Will return a token for the given simulation. The owner (first user/creator) of the simulation
        /// aswell as anyone with the role of type chairman is allowed to call this method.
        /// You can use this method to generate invite links.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("[action]")]
        public async Task<ActionResult<string>> UserToken([FromBody]SimulationUserTokenRequest request)
        {
            var isAllowed = await this._simulationService.IsTokenValidAndUserChairOrOwner(request);
            if (!isAllowed) return BadRequest();
            string token = await _simulationService.GetUserToken(request.UserId);
            if (token == null)
                return NotFound();
            return Ok(token);
        }

        /// <summary>
        /// Returns the user/slotId by the given token.
        /// </summary>
        /// <param name="simsimtoken"></param>
        /// <param name="simulationId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public ActionResult<int> MyUserId([FromHeader]string simsimtoken, int simulationId)
        {
            int? userId = this._simulationService.GetSimulationUserId(simulationId, simsimtoken);
            if (!userId.HasValue) return BadRequest();
            return Ok(userId.Value);
        }
    }
}
