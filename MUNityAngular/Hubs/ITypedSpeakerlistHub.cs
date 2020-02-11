﻿using MUNityAngular.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MUNityAngular.Hubs
{
    public interface ITypedSpeakerlistHub
    {
        Task SpeakerListChanged(SpeakerlistModel speakerlist);

        Task SpeakerTimerStarted(int seconds);

        Task QuestionTimerStarted(int seconds);

        Task TimerStopped();

        Task SpeakerTimerSynced(int seconds);
    }
}
