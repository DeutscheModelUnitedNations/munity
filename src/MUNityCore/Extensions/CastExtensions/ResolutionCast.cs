﻿using MUNity.Models.Resolution;
using MUNity.Models.Resolution.EventArguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MUNityCore.Extensions.CastExtensions
{
    public static class ResolutionCast
    {
        public static MUNity.Models.Resolution.PreambleParagraph ToModel(this MUNityCore.Models.Resolution.SqlResa.ResaPreambleParagraph sourceParagraph)
        {

            MUNity.Models.Resolution.PreambleParagraph model = new MUNity.Models.Resolution.PreambleParagraph()
            {
                Comment = sourceParagraph.Comment,
                Corrected = sourceParagraph.IsCorrected,
                IsLocked = sourceParagraph.IsLocked,
                PreambleParagraphId = sourceParagraph.ResaPreambleParagraphId,
                Text = sourceParagraph.Text
            };
            return model;
        }

        public static MUNity.Models.Resolution.OperativeParagraph ToModel(this MUNityCore.Models.Resolution.SqlResa.ResaOperativeParagraph sourceParagraph)
        {
            if (sourceParagraph.Children == null)
                sourceParagraph.Children = new List<Models.Resolution.SqlResa.ResaOperativeParagraph>();
            var model = new MUNity.Models.Resolution.OperativeParagraph()
            {
                Children = sourceParagraph.Children.Select(n => n.ToModel()).ToList(),
                Comment = sourceParagraph.Comment,
                Corrected = sourceParagraph.Corrected,
                IsLocked = sourceParagraph.IsLocked,
                IsVirtual = sourceParagraph.IsVirtual,
                Name = sourceParagraph.Name,
                OperativeParagraphId = sourceParagraph.ResaOperativeParagraphId,
                Text = sourceParagraph.Text,
                Visible = sourceParagraph.Visible
            };
            return model;
        }

        public static MUNity.Models.Resolution.AddAmendment ToModel(this MUNityCore.Models.Resolution.SqlResa.ResaAddAmendment sourceAmendment)
        {
            var model = new MUNity.Models.Resolution.AddAmendment()
            {
                Activated = sourceAmendment.Activated,
                Id = sourceAmendment.ResaAmendmentId,
                Name = sourceAmendment.GetType().Name,
                SubmitterName = sourceAmendment.SubmitterName,
                SubmitTime = sourceAmendment.SubmitTime,
                TargetSectionId = sourceAmendment.VirtualParagraph.ResaOperativeParagraphId,
                Type = "add"
            };
            return model;
        }

        public static ChangeAmendment ToModel(this MUNityCore.Models.Resolution.SqlResa.ResaChangeAmendment changeAmendment)
        {
            var model = new MUNity.Models.Resolution.ChangeAmendment()
            {
                Activated = changeAmendment.Activated,
                Id = changeAmendment.ResaAmendmentId,
                Name = changeAmendment.GetType().Name,
                NewText = changeAmendment.NewText,
                SubmitterName = changeAmendment.SubmitterName,
                SubmitTime = changeAmendment.SubmitTime,
                TargetSectionId = changeAmendment.TargetParagraph.ResaOperativeParagraphId,
                Type = "change"
            };
            return model;
        }

        public static DeleteAmendment ToModel(this MUNityCore.Models.Resolution.SqlResa.ResaDeleteAmendment sourceAmendment)
        {
            var model = new MUNity.Models.Resolution.DeleteAmendment()
            {
                Activated = sourceAmendment.Activated,
                Id = sourceAmendment.ResaAmendmentId,
                Name = sourceAmendment.GetType().Name,
                SubmitterName = sourceAmendment.SubmitterName,
                SubmitTime = sourceAmendment.SubmitTime,
                TargetSectionId = sourceAmendment.TargetParagraph.ResaOperativeParagraphId,
                Type = "delete"
            };
            return model;
        }

        public static MoveAmendment ToModel(this MUNityCore.Models.Resolution.SqlResa.ResaMoveAmendment sourceAmendment)
        {

            var model = new MUNity.Models.Resolution.MoveAmendment()
            {
                Activated = sourceAmendment.Activated,
                Id = sourceAmendment.ResaAmendmentId,
                Name = sourceAmendment.GetType().Name,
                NewTargetSectionId = sourceAmendment.VirtualParagraph?.ResaOperativeParagraphId ?? "",
                SubmitterName = sourceAmendment.SubmitterName,
                SubmitTime = sourceAmendment.SubmitTime,
                TargetSectionId = sourceAmendment.SourceParagraph?.ResaOperativeParagraphId ?? "",
                Type = sourceAmendment.ResaAmendmentType
            };
            return model;
        }

        public static AddAmendmentCreatedEventArgs ToSocketModel(this MUNityCore.Models.Resolution.SqlResa.ResaAddAmendment amendment, string resolutionId)
        {
            var args = new AddAmendmentCreatedEventArgs()
            {
                ResolutionId = resolutionId,
                Tan = "12345",
                Amendment = new AddAmendment()
                {
                    Activated = amendment.Activated,
                    Id = amendment.ResaAmendmentId,
                    Name = "AddAmendment",
                    SubmitterName = amendment.SubmitterName,
                    SubmitTime = amendment.SubmitTime,
                    TargetSectionId = amendment.VirtualParagraph.ResaOperativeParagraphId,
                    Type = amendment.ResaAmendmentType
                },
                VirtualParagraph = new OperativeParagraph()
                {
                    Children = new List<OperativeParagraph>(),
                    Comment = "",
                    Corrected = false,
                    IsLocked = amendment.VirtualParagraph.IsLocked,
                    IsVirtual = amendment.VirtualParagraph.IsVirtual,
                    Name = "Virtual Paragraph",
                    OperativeParagraphId = amendment.VirtualParagraph.ResaOperativeParagraphId,
                    Text = amendment.VirtualParagraph.Text,
                    Visible = amendment.VirtualParagraph.Visible
                },
                VirtualParagraphIndex = amendment.VirtualParagraph.OrderIndex
            };
            return args;
        }

        public static MoveAmendmentCreatedEventArgs ToSocketModel(this MUNityCore.Models.Resolution.SqlResa.ResaMoveAmendment amendment, string resolutionId)
        {
            var dto = new MoveAmendmentCreatedEventArgs()
            {
                ResolutionId = resolutionId,
                Tan = "12345",
                Amendment = new MoveAmendment()
                {
                    Activated = amendment.Activated,
                    Id = amendment.ResaAmendmentId,
                    Name = "move",
                    NewTargetSectionId = amendment.VirtualParagraph.ResaOperativeParagraphId,
                    SubmitterName = amendment.SubmitterName,
                    SubmitTime = amendment.SubmitTime,
                    TargetSectionId = amendment.SourceParagraph.ResaOperativeParagraphId,
                    Type = amendment.ResaAmendmentType
                },
                VirtualParagraph = new OperativeParagraph()
                {
                    Children = new List<OperativeParagraph>(),
                    Comment = "",
                    Corrected = false,
                    IsLocked = amendment.VirtualParagraph.IsLocked,
                    IsVirtual = amendment.VirtualParagraph.IsVirtual,
                    Name = "virtual",
                    OperativeParagraphId = amendment.VirtualParagraph.ResaOperativeParagraphId,
                    Text = amendment.VirtualParagraph.Text,
                    Visible = amendment.VirtualParagraph.Visible
                },
                VirtualParagraphIndex = amendment.VirtualParagraph.OrderIndex
            };

            return dto;
        }

    }
}
