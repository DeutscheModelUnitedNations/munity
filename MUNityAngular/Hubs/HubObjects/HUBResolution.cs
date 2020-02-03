﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MUNityAngular.Util.Extenstions;

namespace MUNityAngular.Hubs.HubObjects
{
    public class HUBResolution
    {
        public string ID { get; set; }

        public string Name { get; set; }

        public string FullName { get; set; }

        public string Topic { get; set; }
        public string AgendaItem { get; set; }

        public string Session { get; set; }

        public string SubmitterName { get; set; }

        public DateTime Date { get; set; }

        public HUBPreamble Preamble { get; set; }

        public List<HUBChangeAmendment> ChangeAmendments { get; set; }

        public List<HUBDeleteAmendment> DeleteAmendments { get; set; }

        public List<HUBMoveAmendment> MoveAmendments { get; set; }

        public List<HUBAddAmendment> AddAmendmentsSave { get; set; }

        public List<HUBOperativeParagraph> OperativeSections { get; set; }

        public HUBResolution(Models.ResolutionModel resolution)
        {
            this.ID = resolution.ID;
            this.Name = resolution.Name;
            this.FullName = resolution.FullName;
            this.Topic = resolution.Topic;
            this.AgendaItem = resolution.AgendaItem;
            this.Session = resolution.Session;
            this.Date = resolution.Date;
            this.Preamble = new HUBPreamble(resolution.Preamble);
            ChangeAmendments = resolution.ChangeAmendments.ToHubAmendments();
            this.DeleteAmendments = resolution.DeleteAmendments.ToHubAmendments();
            this.MoveAmendments = resolution.MoveAmendments.ToHubAmendments();
            this.AddAmendmentsSave = resolution.AddAmendmentsSave.ToHubAmendments();
            this.OperativeSections = resolution.OperativeSections.ToHubParagraphs();
            this.SubmitterName = SubmitterName;
        }
    }
}
