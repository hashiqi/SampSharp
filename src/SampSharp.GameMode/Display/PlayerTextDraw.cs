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
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.Natives;
using SampSharp.GameMode.Pools;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;

namespace SampSharp.GameMode.Display
{
    /// <summary>
    ///     Represents a player-textdraw.
    /// </summary>
    public class PlayerTextDraw : IdentifiedOwnedPool<PlayerTextDraw>, IIdentifiable, IOwnable<GtaPlayer>
    {
        #region Fields

        /// <summary>
        ///     Gets an ID commonly returned by methods to point out that no textdraw matched the requirements.
        /// </summary>
        public const int InvalidId = Misc.InvalidTextDraw;

        private TextDrawAlignment _alignment;
        private Color _backColor;
        private Color _boxColor;
        private TextDrawFont _font;
        private Color _foreColor;
        private float _height;
        private float _letterHeight;
        private float _letterWidth;
        private int _outline;
        private int _previewModel;
        private int _previewPrimaryColor = -1;
        private Vector _previewRotation;
        private int _previewSecondaryColor = -1;
        private float _previewZoom = 1;
        private bool _proportional;
        private bool _selectable;
        private int _shadow;
        private string _text;
        private bool _useBox;
        private bool _visible;
        private float _width;
        private float _x;
        private float _y;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerTextDraw" /> class.
        /// </summary>
        /// <param name="owner">The owner of the player-textdraw.</param>
        public PlayerTextDraw(GtaPlayer owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            IsApplyFixes = true;
            AutoDestroy = true;

            Id = -1;
            Owner = owner;
            Text = "_";
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerTextDraw" /> class.
        /// </summary>
        /// <param name="owner">The owner of the player-textdraw.</param>
        /// <param name="x">The x-position of the player-textdraw on the screen.</param>
        /// <param name="y">The y-position of the player-textdraw on the screen.</param>
        /// <param name="text">The text of the player-textdraw.</param>
        public PlayerTextDraw(GtaPlayer owner, float x, float y, string text)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            IsApplyFixes = true;
            Id = -1;
            Owner = owner;
            X = x;
            Y = y;
            Text = text;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerTextDraw" /> class.
        /// </summary>
        /// <param name="owner">The owner of the player-textdraw.</param>
        /// <param name="x">The x-position of the player-textdraw on the screen.</param>
        /// <param name="y">The y-position of the player-textdraw on the screen.</param>
        /// <param name="text">The text of the player-textdraw.</param>
        /// <param name="font">The <see cref="TextDrawFont" /> of this textdraw.</param>
        public PlayerTextDraw(GtaPlayer owner, float x, float y, string text, TextDrawFont font)
            : this(owner, x, y, text)
        {
            Font = font;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerTextDraw" /> class.
        /// </summary>
        /// <param name="owner">The owner of the player-textdraw.</param>
        /// <param name="x">The x-position of the player-textdraw on the screen.</param>
        /// <param name="y">The y-position of the player-textdraw on the screen.</param>
        /// <param name="text">The text of the player-textdraw.</param>
        /// <param name="font">The <see cref="TextDrawFont" /> of the player-textdraw.</param>
        /// <param name="foreColor">The foreground <see cref="Color" /> of the player-textdraw.</param>
        public PlayerTextDraw(GtaPlayer owner, float x, float y, string text, TextDrawFont font, Color foreColor)
            : this(owner, x, y, text, font)
        {
            ForeColor = foreColor;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets whether SA-MP fixes should be applied.
        /// </summary>
        public bool IsApplyFixes { get; set; }

        /// <summary>
        ///     Gets or sets whether the textdraw should automatically be destroyed when hidden.
        /// </summary>
        /// <remarks>The textdraw will automatically be recreated once .Show is called.</remarks>
        public bool AutoDestroy { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="TextDrawAlignment" /> of this player-textdraw.
        /// </summary>
        public virtual TextDrawAlignment Alignment
        {
            get { return _alignment; }
            set
            {
                _alignment = value;
                if (Id == -1) return;
                Native.PlayerTextDrawAlignment(Owner.Id, Id, (int) value);
                Update();
            }
        }

        /// <summary>
        ///     Gets or sets the background <see cref="Color" /> of this player-textdraw.
        /// </summary>
        public virtual Color BackColor
        {
            get { return _backColor; }
            set
            {
                _backColor = value;
                if (Id == -1) return;
                Native.PlayerTextDrawBackgroundColor(Owner.Id, Id, value);
                Update();
            }
        }

        /// <summary>
        ///     Gets or sets the foreground <see cref="Color" /> of this player-textdraw.
        /// </summary>
        public virtual Color ForeColor
        {
            get { return _foreColor; }
            set
            {
                _foreColor = value;
                if (Id == -1) return;
                Native.PlayerTextDrawColor(Owner.Id, Id, value);
                Update();
            }
        }

        /// <summary>
        ///     Gets or sets the box <see cref="Color" /> of this player-textdraw.
        /// </summary>
        public virtual Color BoxColor
        {
            get { return _boxColor; }
            set
            {
                _boxColor = value;
                if (Id == -1) return;
                Native.PlayerTextDrawBoxColor(Owner.Id, Id, value);
                Update();
            }
        }

        /// <summary>
        ///     Gets or sets the <see cref="TextDrawFont" /> to use in this player-textdraw.
        /// </summary>
        public virtual TextDrawFont Font
        {
            get { return _font; }
            set
            {
                _font = value;
                if (Id == -1) return;
                Native.PlayerTextDrawFont(Owner.Id, Id, (int) value);
                Update();
            }
        }

        /// <summary>
        ///     Gets or sets the letter-width of this player-textdraw.
        /// </summary>
        public virtual float LetterWidth
        {
            get { return _letterWidth; }
            set
            {
                _letterWidth = value;
                if (Id == -1) return;
                Native.PlayerTextDrawLetterSize(Owner.Id, Id, _letterWidth, _letterHeight);
                Update();
            }
        }

        /// <summary>
        ///     Gets or sets the letter-height of this player-textdraw.
        /// </summary>
        public virtual float LetterHeight
        {
            get { return _letterHeight; }
            set
            {
                _letterHeight = value;
                if (Id == -1) return;
                Native.PlayerTextDrawLetterSize(Owner.Id, Id, _letterWidth, _letterHeight);
                Update();
            }
        }

        /// <summary>
        ///     Gets or sets the outline size of this player-textdraw.
        /// </summary>
        public virtual int Outline
        {
            get { return _outline; }
            set
            {
                _outline = value;
                if (Id == -1) return;
                Native.PlayerTextDrawSetOutline(Owner.Id, Id, value);
                Update();
            }
        }

        /// <summary>
        ///     Gets or sets wheter proporionally space the characters of this player-textdraw.
        /// </summary>
        public virtual bool Proportional
        {
            get { return _proportional; }
            set
            {
                _proportional = value;
                if (Id == -1) return;
                Native.PlayerTextDrawSetProportional(Owner.Id, Id, value);
                Update();
            }
        }

        /// <summary>
        ///     Gets or sets the shadow-size of this player-textdraw.
        /// </summary>
        public virtual int Shadow
        {
            get { return _shadow; }
            set
            {
                _shadow = value;
                if (Id == -1) return;
                Native.PlayerTextDrawSetShadow(Owner.Id, Id, value);
                Update();
            }
        }

        /// <summary>
        ///     Gets or sets the text of this player-textdraw.
        /// </summary>
        public virtual string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                if (Id == -1) return;
                Native.PlayerTextDrawSetString(Owner.Id, Id, value);
                Update();
            }
        }

        /// <summary>
        ///     Gets or sets the x-position of this player-textdraw on the screen.
        /// </summary>
        public virtual float X
        {
            get { return _x; }
            set
            {
                _x = value;
                if (Id == -1) return;
                Refresh();
            }
        }

        /// <summary>
        ///     Gets or sets the y-position of this player-textdraw on the screen.
        /// </summary>
        public virtual float Y
        {
            get { return _y; }
            set
            {
                _y = value;
                if (Id == -1) return;
                Refresh();
            }
        }

        /// <summary>
        ///     Gets or sets the width of this player-textdraw's box.
        /// </summary>
        public virtual float Width
        {
            get { return _width; }
            set
            {
                _width = value;
                if (Id == -1) return;
                Native.PlayerTextDrawTextSize(Owner.Id, Id, _width, _height);
                Update();
            }
        }

        /// <summary>
        ///     Gets or sets the height of this player-textdraw's box.
        /// </summary>
        public virtual float Height
        {
            get { return _height; }
            set
            {
                _height = value;
                if (Id == -1) return;
                Native.PlayerTextDrawTextSize(Owner.Id, Id, _width, _height);
                Update();
            }
        }

        /// <summary>
        ///     Gets or sets whether to draw a box behind the player-textdraw.
        /// </summary>
        public virtual bool UseBox
        {
            get { return _useBox; }
            set
            {
                _useBox = value;
                if (Id == -1) return;
                Native.PlayerTextDrawUseBox(Owner.Id, Id, value);
                Update();
            }
        }

        /// <summary>
        ///     Gets or sets whether this player-textdraw is selectable.
        /// </summary>
        public virtual bool Selectable
        {
            get { return _selectable; }
            set
            {
                _selectable = value;
                if (Id == -1) return;
                Native.PlayerTextDrawSetSelectable(Owner.Id, Id, value);
                Update();
            }
        }

        /// <summary>
        ///     Gets or sets the previewmodel to draw on this player-textdraw.
        /// </summary>
        public virtual int PreviewModel
        {
            get { return _previewModel; }
            set
            {
                _previewModel = value;
                if (Id == -1) return;
                Native.PlayerTextDrawSetPreviewModel(Owner.Id, Id, value);
                Update();
            }
        }

        /// <summary>
        ///     Gets or sets the rotation of this player-textdraw's previewmodel.
        /// </summary>
        public virtual Vector PreviewRotation
        {
            get { return _previewRotation; }
            set
            {
                _previewRotation = value;
                if (Id == -1) return;
                Native.PlayerTextDrawSetPreviewRot(Owner.Id, Id, value.X, value.Y, value.Z, PreviewZoom);
                Update();
            }
        }

        /// <summary>
        ///     Gets or sets the zoom level of this player-textdraw's previewmodel.
        /// </summary>
        public virtual float PreviewZoom
        {
            get { return _previewZoom; }
            set
            {
                _previewZoom = value;
                if (Id == -1) return;
                Native.PlayerTextDrawSetPreviewRot(Owner.Id, Id, PreviewRotation.X, PreviewRotation.Y,
                    PreviewRotation.Z, value);
                Update();
            }
        }

        /// <summary>
        ///     Gets or sets the primary vehicle color of this player-textdraw's previewmodel.
        /// </summary>
        public virtual int PreviewPrimaryColor
        {
            get { return _previewPrimaryColor; }
            set
            {
                _previewPrimaryColor = value;
                if (Id == -1) return;
                Native.PlayerTextDrawSetPreviewVehCol(Owner.Id, Id, _previewPrimaryColor,
                    _previewSecondaryColor);
                Update();
            }
        }

        /// <summary>
        ///     Gets or sets the secondary vehicle color of this player-textdraw's previewmodel.
        /// </summary>
        public virtual int PreviewSecondaryColor
        {
            get { return _previewSecondaryColor; }
            set
            {
                _previewSecondaryColor = value;
                if (Id == -1) return;
                Native.PlayerTextDrawSetPreviewVehCol(Owner.Id, Id, _previewPrimaryColor,
                    _previewSecondaryColor);
                Update();
            }
        }

        /// <summary>
        ///     Gets the textdraw-id of this player-textdraw.
        /// </summary>
        public virtual int Id { get; protected set; }

        /// <summary>
        ///     Gets the owner of this player-textdraw.
        /// </summary>
        public virtual GtaPlayer Owner { get; protected set; }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when the <see cref="BaseMode.OnPlayerClickPlayerTextDraw(GtaPlayer,ClickPlayerTextDrawEventArgs)" /> is being called.
        ///     This callback is called when a player clicks on a player-textdraw.
        /// </summary>
        public event EventHandler<ClickPlayerTextDrawEventArgs> Click;

        #endregion

        #region Methods

        /// <summary>
        ///     Performs tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">Whether managed resources should be disposed.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (Id == -1) return;

            Native.PlayerTextDrawDestroy(Owner.Id, Id);
        }

        /// <summary>
        ///     Displays this player-textdraw to the <see cref="Owner" /> of this textdraw.
        /// </summary>
        public virtual void Show()
        {
            AssertNotDisposed();

            if (Id == -1) Refresh();
            _visible = true;

            Native.PlayerTextDrawShow(Owner.Id, Id);
        }

        /// <summary>
        ///     Hides this player-textdraw.
        /// </summary>
        public virtual void Hide()
        {
            AssertNotDisposed();

            if (Id == -1 || !_visible) return;
            _visible = false;

            Native.PlayerTextDrawHide(Owner.Id, Id);

            if (AutoDestroy)
            {
                Native.PlayerTextDrawDestroy(Owner.Id, Id);
                Id = -1;
            }
        }

        /// <summary>
        ///     Recreates this player-textdraw with all set properties. Called when changing the location on the screen.
        /// </summary>
        protected virtual void Refresh()
        {
            Hide();

            if (Id != -1) Native.PlayerTextDrawDestroy(Owner.Id, Id);
            Id = Native.CreatePlayerTextDraw(Owner.Id, X, Y, FixString(Text));

            //Reset properties
            if (Alignment != default(TextDrawAlignment)) Alignment = Alignment;
            if (BackColor != 0) BackColor = BackColor;
            if (ForeColor != 0) ForeColor = ForeColor;
            if (BoxColor != 0) BoxColor = BoxColor;
            if (Font != default(TextDrawFont)) Font = Font;
            if (LetterWidth != 0) LetterWidth = LetterWidth;
            if (LetterHeight != 0) LetterHeight = LetterHeight;
            if (Outline > 0) Outline = Outline;
            if (Proportional) Proportional = Proportional;
            if (Shadow > 0) Shadow = Shadow;
            if (Width != 0) Width = Width;
            if (Height != 0) Height = Height;
            if (UseBox) UseBox = UseBox;
            if (Selectable) Selectable = Selectable;
            if (PreviewModel != 0) PreviewModel = PreviewModel;
            if (PreviewRotation != Vector.Zero) PreviewRotation = PreviewRotation;
            if (PreviewZoom != 1) PreviewZoom = PreviewZoom;
            if (PreviewPrimaryColor != -1) PreviewPrimaryColor = PreviewPrimaryColor;
            if (PreviewSecondaryColor != -1) PreviewSecondaryColor = PreviewSecondaryColor;

            Update();
        }

        /// <summary>
        ///     Fixes a string so no SA-MP bugs will occur during application.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The fixed string</returns>
        protected virtual string FixString(string input)
        {
            if (!IsApplyFixes) return input;

            //TextDraw string may at max. be 1024 char long
            if (input.Length > 1024)
                input = input.Substring(0, 1024);

            //Empty strings can crash the server
            if (string.IsNullOrEmpty(input))
                input = "_";

            return input.Replace("\n", "~n~");
        }

        /// <summary>
        ///     Updates this textdraw on the client's screen.
        /// </summary>
        protected virtual void Update()
        {
            if (_visible) Show();
        }

        #endregion

        #region Event raisers

        /// <summary>
        ///     Raises the <see cref="Click" /> event.
        /// </summary>
        /// <param name="e">An <see cref="ClickPlayerTextDrawEventArgs" /> that contains the event data. </param>
        public virtual void OnClick(ClickPlayerTextDrawEventArgs e)
        {
            if (Click != null)
                Click(this, e);
        }

        #endregion
    }
}