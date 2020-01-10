﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MUNityAngular.Services;
using MUNityAngular.Util.Extenstions;

namespace MUNityAngular.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResolutionController : ControllerBase
    {
        IHubContext<Hubs.ResolutionHub, Hubs.ITypedResolutionHub> _hubContext;

        public ResolutionController(IHubContext<Hubs.ResolutionHub, Hubs.ITypedResolutionHub> hubContext, [FromServices]ResolutionService service)
        {
            _hubContext = hubContext;
            if (service.HubContext == null)
                service.HubContext = hubContext;
        }

        /// <summary>
        /// Gives an overview of thats possible inside the resolution Controller
        /// </summary>
        /// <returns>a Text that gives a small overview of all posibilities with the Resolution Controller</returns>
        [HttpGet]
        public string Get()
        {
            string description = "Read the Resolution Documentation!";
            return description;
        }

        /// <summary>
        /// Creates a new Resolution
        /// </summary>
        /// <param name="auth">The authenticator-code </param>
        /// <returns>The new created Resolution in a json format.</returns>
        [Route("[action]")]
        [HttpGet]
        public IActionResult Create([FromHeader]string auth, [FromServices]Services.ResolutionService resolutionService,
            [FromServices]AuthService authService)
        {
            if (!authService.CanCreateResolution(auth))
                return StatusCode(StatusCodes.Status403Forbidden, "You are not allowed to do that.");


            string json;
            if (authService.isDefaultAuth(auth))
                json = resolutionService.CreateResolution(true, true).ToJson();
            else
                json = resolutionService.CreateResolution(userid: authService.ValidateAuthKey(auth).userid).ToJson();
            return StatusCode(StatusCodes.Status200OK, json);
        }

        [Route("[action]")]
        [HttpGet]
        public IActionResult AddPreambleParagraph([FromHeader]string auth, [FromHeader]string resolutionid, 
            [FromServices]Services.ResolutionService resolutionService,
            [FromServices]AuthService authService)
        {

            var resolution = resolutionService.GetResolution(resolutionid);
            if (resolution == null)
                return StatusCode(StatusCodes.Status404NotFound, "Document not found or you have no right to do that.");

            if (!authService.CanEditResolution(authService.ValidateAuthKey(auth).userid,resolution))
                return StatusCode(StatusCodes.Status403Forbidden, "You are not allowed to do that.");

            
            if (resolution != null)
            {
                var newPP = resolution.Preamble.AddParagraph();
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(newPP);
                resolutionService.RequestSave(resolution);
                _hubContext.Clients.Group(resolutionid).PreambleParagraphAdded(resolution.Preamble.Paragraphs.IndexOf(newPP), newPP.ID, newPP.Text);
                return StatusCode(StatusCodes.Status200OK, json);
            }

            return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong, this should not have appened");
        }

        /// <summary>
        /// Adds an Premable Paragraph to the Resolution.
        /// </summary>
        /// <param name="auth"></param>
        /// <param name="resolutionid"></param>
        /// <returns></returns>
        [Route("[action]")]
        [HttpGet]
        public IActionResult AddOperativeParagraph([FromHeader]string auth, [FromHeader]string resolutionid, 
            [FromServices]ResolutionService resolutionService,
            [FromServices]AuthService authService)
        {
            var resolution = resolutionService.GetResolution(resolutionid);
            if (resolution == null)
                return StatusCode(StatusCodes.Status404NotFound, "Document not found or you have no right to do that.");

            if (!authService.CanEditResolution(authService.ValidateAuthKey(auth).userid, resolution))
                return StatusCode(StatusCodes.Status403Forbidden, "You are not allowed to do that.");

            if (resolution != null)
            {
                var newPP = resolution.AddOperativeParagraph();
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(newPP);
                resolutionService.RequestSave(resolution);
                return StatusCode(StatusCodes.Status200OK, json);
            }

            return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong, this should not have appened");

        }

        [Route("[action]")]
        [HttpGet]
        public IActionResult ChangePreambleParagraph([FromHeader]string auth, [FromHeader]string resolutionid, 
            [FromHeader]string paragraphid, [FromHeader]string newtext, 
            [FromServices]Services.ResolutionService resolutionService,
            [FromServices]AuthService authService)
        {
            var re = Request;
            var headers = re.Headers;
            if (newtext.EndsWith('|'))
                newtext = newtext.Substring(0, newtext.Length - 1);

            var resolution = resolutionService.GetResolution(resolutionid);
            if (resolution == null)
                return StatusCode(StatusCodes.Status404NotFound, "Document not found or you have no right to do that.");

            if (!authService.CanEditResolution(authService.ValidateAuthKey(auth).userid, resolution))
                return StatusCode(StatusCodes.Status403Forbidden, "You are not allowed to do that.");

            if (resolution != null)
            {
                var newPP = resolution.Preamble.Paragraphs.FirstOrDefault(n => n.ID == paragraphid);
                if (newPP != null)
                {
                    newPP.Text = newtext;
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(newPP);
                    resolutionService.RequestSave(resolution);
                    _hubContext.Clients.Group(resolutionid).PreambleParagraphChanged(newPP.ID, newPP.Text);
                    return StatusCode(StatusCodes.Status200OK, json);
                }
                else
                {
                    return StatusCode(StatusCodes.Status404NotFound, "Preamble Paragraph not found");
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status404NotFound, "Resolution not found");
            }
        }

        [Route("[action]")]
        [HttpGet]
        public IActionResult ChangeOperativeParagraph([FromHeader]string auth, [FromHeader]string resolutionid, 
            [FromHeader]string paragraphid, [FromHeader]string newtext, 
            [FromServices]ResolutionService resolutionService,
            [FromServices]AuthService authService)
        {
            var resolution = resolutionService.GetResolution(resolutionid);
            if (resolution == null)
                return StatusCode(StatusCodes.Status404NotFound, "Document not found or you have no right to do that.");

            if (!authService.CanEditResolution(authService.ValidateAuthKey(auth).userid, resolution))
                return StatusCode(StatusCodes.Status403Forbidden, "You are not allowed to do that.");

            if (resolution != null)
            {
                var newPP = resolution.OperativeSections.FirstOrDefault(n => n.ID == paragraphid);
                if (newPP != null)
                {
                    newPP.Text = newtext;
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(newPP);
                    return StatusCode(StatusCodes.Status200OK, json);
                }
                else
                {
                    return StatusCode(StatusCodes.Status404NotFound, "Operative Paragraph not found");
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status404NotFound, "Resolution not found");
            }
        }

        
        [Route("[action]")]
        [HttpGet]
        public IActionResult Get([FromHeader]string auth, [FromHeader]string id,
            [FromServices]ResolutionService resolutionService,
            [FromServices]AuthService authService)
        {
            var resolution = resolutionService.GetResolution(id);
            if (resolution == null)
                return StatusCode(StatusCodes.Status404NotFound, "Document not found or you have no right to do that.");

            if (!authService.CanEditResolution(authService.ValidateAuthKey(auth).userid, resolution))
                return StatusCode(StatusCodes.Status403Forbidden, "You are not allowed to do that.");

            return StatusCode(StatusCodes.Status200OK, resolution.ToJson());
            
        }

        // PUT: api/Resolution/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
