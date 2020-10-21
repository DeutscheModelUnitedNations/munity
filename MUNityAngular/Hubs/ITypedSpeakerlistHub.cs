﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MUNityCore.Models;

namespace MUNityCore.Hubs
{

    /// <summary>
    /// The SpeakerlistHub defines every method that is needed to directly communicate from the
    /// Serverto the Client via a WebSocket.
    /// </summary>
    public interface ITypedSpeakerlistHub
    {
        Task SpeakerListChanged(SpeakerlistModel speakerlist);

        Task SpeakerTimerStarted(int seconds);

        Task QuestionTimerStarted(int seconds);

        Task TimerStopped();

        Task SpeakerTimerSynced(int seconds);
    }
}
