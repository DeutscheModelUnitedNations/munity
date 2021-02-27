﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MUNityCore.Models.Resolution.V2;
using MUNityCore.Services;
using MUNity.Models.Resolution;
using MUNity.Extensions.ResolutionExtensions;
using MUNity.Models.Resolution.EventArguments;

namespace MUNityCore.Controllers
{

    /// <summary>
    /// The ResolutionController offers endpoints to work with resolution documents.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ResolutionController : ControllerBase
    {
        readonly IHubContext<Hubs.ResolutionHub, MUNity.Hubs.ITypedResolutionHub> _hubContext;

        private readonly IResolutionService _resolutionService;

        private readonly IAuthService _authService;

        private enum EPostAmendmentMode
        {
            NotAllowed,
            AllowedPost,
            AllowedRequest
        }

        public ResolutionController(IHubContext<Hubs.ResolutionHub, MUNity.Hubs.ITypedResolutionHub> hubContext, 
            IResolutionService resolutionService,
                IAuthService authService)
        {
            _hubContext = hubContext;
            _resolutionService = resolutionService;
            _authService = authService;
        }

        [Route("[action]")]
        [HttpGet]
        [AllowAnonymous]
        public ActionResult IsUp()
        {
            return Ok();
        }

        #region "Resolution Creation"

        /// <summary>
        /// Create a public accessable resolution.
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        [Route("[action]")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<Resolution>> CreatePublic(string title)
        {
            return await _resolutionService.CreatePublicResolution(title);
        }

        #endregion

        /// <summary>
        /// Returns a resolution with the given Id if the user is allowed to read the resolution or it is public.
        /// </summary>
        /// <param name="password">An optional password that is checked againts the EditPassword.</param>
        /// <param name="id">The Id of the resolution (not the public id)</param>
        /// <returns></returns>
        [Route("[action]")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<Resolution>> GetResolution([FromHeader]string password, string id)
        {
            if (!await CanUserReadResolution(id, password))
                return Forbid();
            var resolution = await this._resolutionService.GetResolution(id);
            if (resolution == null) return NotFound();

            return Ok(resolution);
        }

        /// <summary>
        /// Looks up a resolution and checks if it exists.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("[action]")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> ResolutionExists(string id)
        {
            return await this._resolutionService.ResolutionExists(id);
        }

        [Route("[action]")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> CanEditResolution(string id)
        {
            return await CanUserEditResolution(id);
        }

        #region "Preamble"

        [Route("[action]")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> AddPreambleParagraph([FromBody]PreambleParagraphAddedEventArgs body)
        {
            if (!await CanUserEditResolution(body.ResolutionId))
                return Forbid();

            var resolution = await this._resolutionService.GetResolution(body.ResolutionId);
            if (resolution.ResolutionId == null) return NotFound();
            if (body.Paragraph == null) return BadRequest();

            if (resolution.Preamble.Paragraphs.Any(n => n.PreambleParagraphId == body.Paragraph.PreambleParagraphId)) return Ok();

            if (string.IsNullOrEmpty(body.Paragraph.PreambleParagraphId))
                body.Paragraph.PreambleParagraphId = Guid.NewGuid().ToString();

            resolution.Preamble.Paragraphs.Add(body.Paragraph);
            _ = _resolutionService.SaveResolution(resolution).ConfigureAwait(false);
            _ = this._hubContext.Clients.Group(body.ResolutionId).PreambleParagraphAdded(body);
            return Ok();
        }

        [Route("[action]")]
        [HttpPut]
        [AllowAnonymous]
        public async Task<ActionResult> ChangePreambleParagraphText([FromBody]PreambleParagraphTextChangedEventArgs body)
        {
            if (!await CanUserEditResolution(body.ResolutionId))
                return Forbid();

            var resolution = await this._resolutionService.GetResolution(body.ResolutionId);
            if (resolution.ResolutionId == null) return NotFound();
            var targetParagraph = resolution.Preamble?.Paragraphs.FirstOrDefault(n => n.PreambleParagraphId == body.ParagraphId);
            if (targetParagraph == null) return NotFound();
            targetParagraph.Text = body.Text;

            _ = _resolutionService.SaveResolution(resolution).ConfigureAwait(false);
            _ = this._hubContext.Clients.Group(body.ResolutionId).PreambleParagraphTextChanged(body);
            return Ok();
        }

        [Route("[action]")]
        [HttpPut]
        [AllowAnonymous]
        public async Task<ActionResult> ChangePreambleParagraphCommentText([FromBody]PreambleParagraphCommentTextChangedEventArgs body)
        {
            if (!await CanUserEditResolution(body.ResolutionId))
                return Forbid();

            var resolution = await this._resolutionService.GetResolution(body.ResolutionId);
            if (resolution.ResolutionId == null) return NotFound();
            var targetParagraph = resolution.Preamble?.Paragraphs.FirstOrDefault(n => n.PreambleParagraphId == body.PreambleParagraphId);
            if (targetParagraph == null) return NotFound();
            // Ingore the differenz comments for now and change the Comment Property in the preamble paragraph
            //var targetComment = targetParagraph.Comments.FirstOrDefault(n => n.CommentId == body.CommentId);
            //if (targetComment == null) return NotFound();
            //targetComment.Text = body.Text;

            targetParagraph.Comment = body.Text;

            _ = _resolutionService.SaveResolution(resolution).ConfigureAwait(false);
            _ = this._hubContext.Clients.Group(body.ResolutionId).PreambleParagraphCommentTextChanged(body);
            return Ok();
        }

        [Route("[action]")]
        [HttpPut]
        [AllowAnonymous]
        public async Task<ActionResult> DeleteResolutionPreambleParagraph([FromBody]PreambleParagraphRemovedEventArgs args)
        {
            if (!await CanUserEditResolution(args.ResolutionId))
                return Forbid();

            var resolution = await this._resolutionService.GetResolution(args.ResolutionId);
            if (resolution.ResolutionId == null) return NotFound();
            var targetParagraph = resolution.Preamble?.Paragraphs.FirstOrDefault(n => n.PreambleParagraphId == args.PreambleParagraphId);
            if (targetParagraph == null) return NotFound();

            resolution.Preamble.Paragraphs.Remove(targetParagraph);

            _ = _resolutionService.SaveResolution(resolution).ConfigureAwait(false);
            _ = this._hubContext.Clients.Group(args.ResolutionId).PreambleParagraphRemoved(args);
            return Ok();
        }

        #endregion

        [Route("[action]")]
        [HttpPut]
        [AllowAnonymous]
        public async Task<ActionResult> UpdateOperativeParagraph([FromBody]OperativeParagraphChangedEventArgs body)
        {
            if (!await CanUserEditResolution(body.ResolutionId))
                return Forbid();

            var resolution = await this._resolutionService.GetResolution(body.ResolutionId);
            if (resolution == null) return NotFound("Resolution with the given id not found!");
            var targetParagraph = resolution.OperativeSection?.FirstOrDefault(n => n.OperativeParagraphId == body.Paragraph.OperativeParagraphId);
            if (targetParagraph == null) return NotFound("Operative Paragraph not found");
            if (targetParagraph.GetHashCode() == body.Paragraph.GetHashCode()) 
                return Ok();

            var result = await this._resolutionService.UpdateOperativeParagraph(resolution, body.Paragraph);
            if (result)
            {
                _ = this._hubContext.Clients.Group(body.ResolutionId).OperativeParagraphChanged(body).ConfigureAwait(false);
                return Ok();
            }
            return Problem(detail: "Unable to update the operative paragraph because it may wasnt found");
        }

        [Route("[action]")]
        [HttpPut]
        [AllowAnonymous]
        public async Task<ActionResult> UpdateOperativeSection([FromBody] OperativeSectionChangedEventArgs body)
        {
            if (!await CanUserEditResolution(body.ResolutionId))
                return Forbid();

            var resolution = await this._resolutionService.GetResolution(body.ResolutionId);
            if (resolution == null) return NotFound("Resolution with the given id not found!");
            if (body.Section.GetHashCode() == resolution.OperativeSection.GetHashCode()) return Ok();

            resolution.OperativeSection = body.Section;
            _ = this._resolutionService.SaveResolution(resolution);
            _ = this._hubContext.Clients.Group(body.ResolutionId).OperativeSectionChanged(body).ConfigureAwait(false);
            return Ok();
        }

        [Route("[action]")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> PostDeleteAmendment(string resolutionId, string tan, [FromBody]DeleteAmendment amendment)
        {
            var mode = await CanUserSubmitAmendments(resolutionId);
            if (mode == EPostAmendmentMode.NotAllowed) return Forbid();

            if (mode == EPostAmendmentMode.AllowedPost)
            {
                var resolution = await this._resolutionService.GetResolution(resolutionId);
                if (resolution == null) return NotFound("Resolution not found.");
                
                if (!CanAmendmentBeAdded(amendment, resolution.OperativeSection))
                    return BadRequest();

                resolution.OperativeSection.DeleteAmendments.Add(amendment);

                var updated = await this._resolutionService.SaveResolution(resolution);
                await _hubContext.Clients.Groups(updated.ResolutionId)
                    .ResolutionChanged(new ResolutionChangedArgs(tan, updated));
                return Ok();
            }

            return BadRequest();
        }


        ///// <summary>
        ///// Puts the user into the signalR Group for this document/resolution.
        ///// </summary>
        ///// <param name="auth"></param>
        ///// <param name="id"></param>
        ///// <param name="connectionid"></param>
        ///// <param name="resolutionService"></param>
        ///// <param name="authService"></param>
        ///// <returns></returns>
        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> SubscribeToResolution(string resolutionid,string connectionid)
        {
            var resolution = _resolutionService.GetResolution(resolutionid);
            if (resolution == null) return NotFound();

            var infoModel = await _resolutionService.GetResolutionAuth(resolutionid);

            var canRead = await CanUserReadResolution(resolutionid);
            if (!canRead) return Forbid();

            await _hubContext.Groups.AddToGroupAsync(connectionid, resolutionid);

            return StatusCode(StatusCodes.Status200OK);
        }

        private async Task<bool> CanUserEditResolution(string id, string password = null)
        {
            return true;    // TODO Remove this when done with testing
            var resolutionAuth = await this._resolutionService.GetResolutionAuth(id);
            if (resolutionAuth == null) return false;
            if (resolutionAuth.AllowPublicEdit) return true;
            // Check if the resolution has an edit password
            // do not set it to return password == resolutionUath.EditPassword
            // because the password can be wrong or not given but the user is
            // in the whitelist of the document and can authenticate in the next step!
            if (!string.IsNullOrEmpty(resolutionAuth.EditPassword))
                if (password == resolutionAuth.EditPassword) return true;
            // The resolution is not public and no User was found. So the user is not allowed to edit this document!
            if (User == null) return false;
            var user = this._authService.GetUserOfClaimPrincipal(User);
            if (user == null) return false;
            return resolutionAuth.Users.Any(n => n.User.MunityUserId == user.MunityUserId && n.CanWrite);
        }

        private async Task<bool> CanUserReadResolution(string id, string password = null)
        {
            var resolutionAuth = await this._resolutionService.GetResolutionAuth(id);
            if (resolutionAuth == null) return false;
            if (resolutionAuth.AllowPublicRead) return true;
            // The resolution is protected by a reading password!
            if (!string.IsNullOrEmpty(resolutionAuth.ReadPassword))
                if (password == resolutionAuth.EditPassword) return true;
            // The resolution is not public and no User was found. So the user is not allowed to edit this document!
            if (User == null) return false;
            var user = this._authService.GetUserOfClaimPrincipal(User);
            if (user == null) return false;
            return resolutionAuth.Users.Any(n => n.User.MunityUserId == user.MunityUserId && n.CanRead);
        }

        private bool CanAmendmentBeAdded(AbstractAmendment amendment, OperativeSection operativeSection)
        {
            if (operativeSection == null) return false;
            if (amendment == null) return false;
            if (string.IsNullOrWhiteSpace(amendment.Id))
                amendment.Id = Guid.NewGuid().ToString();

            if (amendment is DeleteAmendment deleteAmendment)
            {
                if (operativeSection.DeleteAmendments.Any(n => n.Id == amendment.Id)) return false;
                var targetParagraph = operativeSection.FirstOrDefault(n => n.OperativeParagraphId == deleteAmendment.TargetSectionId);
                if (targetParagraph == null) return false;
                if (operativeSection.Paragraphs.All(n => n.OperativeParagraphId != amendment.TargetSectionId)) return false;
            }
            if (amendment is ChangeAmendment changeAmendment)
            {
                if (operativeSection.DeleteAmendments.Any(n => n.Id == amendment.Id)) return false;
                var targetParagraph = operativeSection.FirstOrDefault(n => n.OperativeParagraphId == changeAmendment.TargetSectionId);
                if (targetParagraph == null) return false;
                if (string.IsNullOrWhiteSpace(changeAmendment.NewText)) return false;
            }
            if (amendment is AddAmendment addAmendment)
            {
                if (operativeSection.AddAmendments.Any(n => n.Id == amendment.Id)) return false;
                var targetParagraph = operativeSection.FirstOrDefault(n => n.OperativeParagraphId == addAmendment.TargetSectionId);
                if (targetParagraph == null) return false;
            }

            return true;
        } 
    }
}
