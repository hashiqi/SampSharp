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

using System;
using System.Threading;
using SampSharp.GameMode.Tools;

namespace SampSharp.GameMode.Controllers
{
    /// <summary>
    ///     A controller processing sync requests.
    /// </summary>
    public sealed class SyncController : IEventListener
    {
        private static BaseMode _gameMode;
        private static bool _waiting;

        /// <summary>
        ///     Gets the main thread.
        /// </summary>
        public static Thread MainThread { get; private set; }

        /// <summary>
        ///     Registers the events this SyncController wants to listen to.
        /// </summary>
        /// <param name="gameMode">The running GameMode.</param>
        public void RegisterEvents(BaseMode gameMode)
        {
            _gameMode = gameMode;
            MainThread = Thread.CurrentThread;

            gameMode.Exited += (sender, args) =>
            {
                foreach (Sync.SyncTask t in Sync.SyncTask.All)
                {
                    t.Dispose();
                }
            };
        }

        /// <summary>
        ///     Start waiting for a tick to sync all resync requests.
        /// </summary>
        public static void Start()
        {
            if (!_waiting)
            {
                _waiting = true;
                _gameMode.Tick += _gameMode_Tick;
            }
        }

        private static void _gameMode_Tick(object sender, EventArgs e)
        {
            foreach (Sync.SyncTask t in Sync.SyncTask.All)
            {
                t.Run();
                t.Dispose();
            }

            _waiting = false;
            _gameMode.Tick -= _gameMode_Tick;
        }
    }
}