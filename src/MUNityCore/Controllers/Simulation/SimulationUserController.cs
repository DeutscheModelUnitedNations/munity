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

namespace MUNityCore.Controllers.Simulation
{
    [Route("api/Simulation/User")]
    [ApiController]
    [EnableCors("munity")]
    public class SimulationUserController : ControllerBase
    {
        public IHubContext<SimulationHub, ITypedSimulationHub> HubContext { get; set; }

        private Services.SimulationService _simulationService;

        public SimulationUserController(IHubContext<SimulationHub, ITypedSimulationHub> hubContext, Services.SimulationService simulationService)
        {
            this.HubContext = hubContext;
            this._simulationService = simulationService;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult<SimulationUserAdminDto>> CreateUser([FromBody]SimulationRequest body)
        {
            var isAllowed = await this._simulationService.IsTokenValidAndUserChairOrOwner(body.SimulationId, body.Token);
            if (!isAllowed) return BadRequest();
            var newUser = this._simulationService.CreateUser(body.SimulationId, "");
            if (newUser == null)
                return NotFound();
            return Ok(newUser.ToSimulationUserAdminDto());
        }

        [HttpPut]
        [Route("[action]")]
        public async Task<ActionResult<string>> UserToken([FromBody]SimulationUserTokenRequest request)
        {
            var isAllowed = await this._simulationService.IsTokenValidAndUserChairOrOwner(request);
            if (!isAllowed) return BadRequest();
            var db = _simulationService.GetDatabaseInstance();
            var token = db.SimulationUser.FirstOrDefault(n => n.Simulation.SimulationId == request.SimulationId &&
            n.SimulationUserId == request.UserId).Token;
            if (token == null)
                return NotFound();
            return Ok(token);
        }

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
