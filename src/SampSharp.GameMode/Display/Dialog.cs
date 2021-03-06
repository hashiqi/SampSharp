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
using System.Collections.Generic;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.Natives;
using SampSharp.GameMode.World;

namespace SampSharp.GameMode.Display
{
    /// <summary>
    ///     Represents a SA:MP dialog.
    /// </summary>
    public class Dialog
    {
        #region Fields

        /// <summary>
        ///     Gets the ID this system will use.
        /// </summary>
        protected const int DialogId = 10000;

        /// <summary>
        ///     Gets the ID this system will use to hide Dialogs.
        /// </summary>
        protected const int DialogHideId = -1;

        /// <summary>
        ///     Contains all instances of Dialogs that are being shown to Players.
        /// </summary>
        protected static Dictionary<int, Dialog> OpenDialogs = new Dictionary<int, Dialog>();

        /// <summary>
        ///     Gets all opened dialogs.
        /// </summary>
        public static IEnumerable<Dialog> All
        {
            get { return OpenDialogs.Values; }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the Dialog class.
        /// </summary>
        /// <param name="style">The style of the dialog.</param>
        /// <param name="caption">
        ///     The title at the top of the dialog. The length of the caption can not exceed more than 64
        ///     characters before it starts to cut off.
        /// </param>
        /// <param name="message">The text to display in the main dialog. Use \n to start a new line and \t to tabulate.</param>
        /// <param name="button">The text on the left button.</param>
        public Dialog(DialogStyle style, string caption, string message, string button)
        {
            Style = style;
            Caption = caption;
            Message = message;
            Button1 = button;
        }

        /// <summary>
        ///     Initializes a new instance of the Dialog class.
        /// </summary>
        /// <param name="style">The style of the dialog.</param>
        /// <param name="caption">
        ///     The title at the top of the dialog. The length of the caption can not exceed more than 64
        ///     characters before it starts to cut off.
        /// </param>
        /// <param name="message">The text to display in the main dialog. Use \n to start a new line and \t to tabulate.</param>
        /// <param name="button1">The text on the left button.</param>
        /// <param name="button2">The text on the right button. Leave it blank to hide it.</param>
        public Dialog(DialogStyle style, string caption, string message, string button1, string button2)
        {
            Style = style;
            Caption = caption;
            Message = message;
            Button1 = button1;
            Button2 = button2;
        }

        /// <summary>
        ///     Initializes a new instance of the Dialog class with the <see cref="DialogStyle.List" />.
        /// </summary>
        /// <param name="caption">
        ///     The title at the top of the dialog. The length of the caption can not exceed more than 64
        ///     characters before it starts to cut off.
        /// </param>
        /// <param name="lines">The lines to display in the list dialog.</param>
        /// <param name="button">The text on the button.</param>
        public Dialog(string caption, string[] lines, string button)
        {
            Style = DialogStyle.List;
            Caption = caption;
            Message = string.Join("\n", lines);
            Button1 = button;
        }

        /// <summary>
        ///     Initializes a new instance of the Dialog class with the <see cref="DialogStyle.List" />.
        /// </summary>
        /// <param name="caption">
        ///     The title at the top of the dialog. The length of the caption can not exceed more than 64
        ///     characters before it starts to cut off.
        /// </param>
        /// <param name="lines">The lines to display in the list dialog.</param>
        /// <param name="button1">The text on the left button.</param>
        /// <param name="button2">The text on the right button. Leave it blank to hide it.</param>
        public Dialog(string caption, string[] lines, string button1, string button2)
        {
            Style = DialogStyle.List;
            Caption = caption;
            Message = string.Join("\n", lines);
            Button1 = button1;
            Button2 = button2;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     The style of the dialog.
        /// </summary>
        public DialogStyle Style { get; set; }

        /// <summary>
        ///     The title at the top of the dialog. The length of the caption can not exceed more than 64 characters before it
        ///     starts to cut off.
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        ///     The text to display in the main dialog. Use \n to start a new line and \t to tabulate.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        ///     The text on the left button.
        /// </summary>
        public string Button1 { get; set; }

        /// <summary>
        ///     The text on the right button. Leave it blank to hide it.
        /// </summary>
        public string Button2 { get; set; }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when the <see cref="BaseMode.OnDialogResponse(GtaPlayer,DialogResponseEventArgs)" /> is being called.
        ///     This callback is called when a player responds to a dialog by either clicking a button, pressing ENTER/ESC or
        ///     double-clicking a list item (if using a <see cref="DialogStyle.List" />).
        ///     This callback is called when a player connects to the server.
        /// </summary>
        public event EventHandler<DialogResponseEventArgs> Response;

        #endregion

        #region Methods

        /// <summary>
        ///     Shows the dialog box to a Player.
        /// </summary>
        /// <param name="player">The Player to show the dialog to.</param>
        public virtual void Show(GtaPlayer player)
        {
            if (player == null)
                throw new ArgumentNullException("player");

            OpenDialogs[player.Id] = this;

            Native.ShowPlayerDialog(player.Id, DialogId, (int) Style, Caption, Message, Button1,
                Button2 ?? string.Empty);
        }

        /// <summary>
        ///     Hides all dialogs for a Player.
        /// </summary>
        /// <param name="player">The Player to hide all dialogs from.</param>
        public static void Hide(GtaPlayer player)
        {
            if (player == null)
                throw new ArgumentNullException("player");

            if (OpenDialogs.ContainsKey(player.Id))
                OpenDialogs.Remove(player.Id);

            Native.ShowPlayerDialog(player.Id, DialogHideId, (int) DialogStyle.MessageBox, string.Empty,
                string.Empty, string.Empty, string.Empty);
        }

        /// <summary>
        ///     Gets the dialog currently being shown to a Player.
        /// </summary>
        /// <param name="player">The Player whose Dialog you want.</param>
        /// <returns>The Dialog currently being shown to the Player.</returns>
        public static Dialog GetOpenDialog(GtaPlayer player)
        {
            if (player == null)
                throw new ArgumentNullException("player");

            return OpenDialogs.ContainsKey(player.Id) ? OpenDialogs[player.Id] : null;
        }

        /// <summary>
        ///     Raises the <see cref="Response" /> event.
        /// </summary>
        /// <param name="e">An <see cref="DialogResponseEventArgs" /> that contains the event data. </param>
        public virtual void OnResponse(DialogResponseEventArgs e)
        {
            if (OpenDialogs.ContainsKey(e.Player.Id))
                OpenDialogs.Remove(e.Player.Id);

            if (Response != null)
                Response(this, e);
        }

        #endregion
    }
}