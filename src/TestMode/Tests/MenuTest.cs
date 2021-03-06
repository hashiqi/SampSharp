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

using SampSharp.GameMode.Display;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;
using SampSharp.GameMode.World;

namespace TestMode.Tests
{
    public class MenuTest : ITest
    {
        public void Start(GameMode gameMode)
        {
        }

        [Command("menu")]
        public static bool MenuCommand(GtaPlayer player)
        {
            var m = new Menu("Test menu", 0, 0);

            m.Columns.Add(new MenuColumn(100));

            m.Rows.Add(new MenuRow("Active"));
            m.Rows.Add(new MenuRow("Disabled", true));
            m.Rows.Add(new MenuRow("Active2"));

            m.Show(player);

            m.Exit += (o, eventArgs) =>
            {
                player.SendClientMessage(Color.Red, "MENU CLOSED");
                m.Dispose();
            };

            m.Response += (o, eventArgs) =>
            {
                player.SendClientMessage(Color.Green, "SELECTED ROW " + eventArgs.Row);
                m.Dispose();
            };

            return true;
        }
    }
}