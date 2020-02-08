﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace MUNityAngular.Services
{
    public class SpeakerlistService
    {
        Random _rnd = new Random();
        private List<Models.SpeakerlistModel> Speakerlists;

        private Timer countDownTimer;

        public Models.SpeakerlistModel CreateSpeakerlist()
        {
            int id = _rnd.Next(100000, 999999);
            while (Speakerlists.Any(n => n.PublicId == id))
            {
                id = _rnd.Next(100000, 999999);
            }
            var speakerList = new Models.SpeakerlistModel();
            speakerList.PublicId = id;
            Speakerlists.Add(speakerList);
            return speakerList;
        }

        public Models.SpeakerlistModel GetSpeakerlist(string id)
        {
            return Speakerlists.FirstOrDefault(n => n.ID == id);
        }

        public Models.SpeakerlistModel GetSpeakerlistByPublicId(int publicId)
        {
            return Speakerlists.FirstOrDefault(n => n.PublicId == publicId);
        }

        public SpeakerlistService()
        {
            Speakerlists = new List<Models.SpeakerlistModel>();
            this.countDownTimer = new Timer(1000);
            this.countDownTimer.Elapsed += CountDownTimer_Elapsed;
            this.countDownTimer.Start();
            Console.WriteLine("Speakerlist Service Started!");
        }

        private void CountDownTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            TimeSpan oneSec = new TimeSpan(0, 0, 1);
            foreach (var speakerlist in this.Speakerlists)
            {
                if (speakerlist.IsSpeaking)
                {
                    speakerlist.RemainingSpeakerTime = speakerlist.RemainingSpeakerTime - oneSec;
                }
                if (speakerlist.IsQuestioning)
                {
                    speakerlist.RemainingQuestionTime = speakerlist.RemainingQuestionTime - oneSec;
                }
            }
        }
    }
}
