﻿// SampSharp
// Copyright 2015 Tim Potze
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using SampSharp.GameMode.SAMP;

namespace SampSharp.GameMode.Controllers
{
    /// <summary>
    ///     A controller processing delays.
    /// </summary>
    public class DelayController : IEventListener
    {
        /// <summary>
        ///     Registers the events this DelayController wants to listen to.
        /// </summary>
        /// <param name="gameMode">The running GameMode.</param>
        public void RegisterEvents(BaseMode gameMode)
        {
            gameMode.TimerTick += (sender, args) =>
            {
                var delay = sender as Delay;

                if (delay != null && delay.Action != null)
                    delay.Action();
            };
        }
    }
}