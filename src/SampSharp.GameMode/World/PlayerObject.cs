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

namespace SampSharp.GameMode.World
{
    /// <summary>
    ///     Represents a player-object.
    /// </summary>
    public class PlayerObject : IdentifiedOwnedPool<PlayerObject>, IGameObject, IOwnable<GtaPlayer>, IIdentifiable
    {
        #region Fields

        /// <summary>
        ///     The invalid identifier.
        /// </summary>
        public const int InvalidId = Misc.InvalidObjectId;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the rotation of this <see cref="IGameObject" />.
        /// </summary>
        public virtual Vector Rotation
        {
            get
            {
                float x, y, z;
                Native.GetPlayerObjectRot(Owner.Id, Id, out x, out y, out z);
                return new Vector(x, y, z);
            }
            set { Native.SetPlayerObjectRot(Owner.Id, Id, value.X, value.Y, value.Z); }
        }

        /// <summary>
        ///     Gets the position of this <see cref="IWorldObject" />.
        /// </summary>
        public virtual Vector Position
        {
            get
            {
                float x, y, z;
                Native.GetPlayerObjectPos(Owner.Id, Id, out x, out y, out z);
                return new Vector(x, y, z);
            }
            set { Native.SetPlayerObjectPos(Owner.Id, Id, value.X, value.Y, value.Z); }
        }

        /// <summary>
        ///     Gets whether this <see cref="IGameObject" /> is moving.
        /// </summary>
        public virtual bool IsMoving
        {
            get { return Native.IsPlayerObjectMoving(Owner.Id, Id); }
        }

        /// <summary>
        ///     Gets whether this <see cref="IGameObject" /> is valid.
        /// </summary>
        public virtual bool IsValid
        {
            get { return Native.IsValidPlayerObject(Owner.Id, Id); }
        }

        /// <summary>
        ///     Gets the model of this <see cref="IGameObject" />.
        /// </summary>
        public virtual int ModelId { get; private set; }

        /// <summary>
        ///     Gets the draw distance of this <see cref="IGameObject" />.
        /// </summary>
        public virtual float DrawDistance { get; private set; }

        /// <summary>
        /// Gets the Identity of this <see cref="IIdentifiable" />.
        /// </summary>
        public virtual int Id { get; private set; }

        /// <summary>
        /// Gets the owner of this <see cref="PlayerObject" />.
        /// </summary>
        public virtual GtaPlayer Owner { get; private set; }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when the <see cref="OnMoved" /> callback is being called.
        ///     This callback is called when an object is moved after <see cref="Move(Vector,float)" /> (when it stops moving).
        /// </summary>
        public event EventHandler<EventArgs> Moved;

        /// <summary>
        ///     Occurs when the <see cref="OnSelected" /> callback is being called.
        ///     This callback is called when a player selects an object after <see cref="Native.SelectObject" /> has been used.
        /// </summary>
        public event EventHandler<SelectPlayerObjectEventArgs> Selected;

        /// <summary>
        ///     Occurs when the <see cref="OnEdited" /> callback is being called.
        ///     This callback is called when a player ends object edition mode.
        /// </summary>
        public event EventHandler<EditPlayerObjectEventArgs> Edited;

        #endregion

        #region Constructor

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerObject" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public PlayerObject(int id) : this(GtaPlayer.Find(id/(Limits.MaxObjects + 1)), id%(Limits.MaxObjects + 1))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerObject" /> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="id">The identifier.</param>
        /// <exception cref="System.ArgumentNullException">owner</exception>
        public PlayerObject(GtaPlayer owner, int id)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            Owner = owner;
            Id = id;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerObject" /> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="modelid">The modelid.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        public PlayerObject(GtaPlayer owner, int modelid, Vector position, Vector rotation)
            : this(owner, modelid, position, rotation, 0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerObject" /> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="modelid">The modelid.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        /// <param name="drawDistance">The draw distance.</param>
        /// <exception cref="System.ArgumentNullException">owner</exception>
        public PlayerObject(GtaPlayer owner, int modelid, Vector position, Vector rotation, float drawDistance)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            Owner = owner;
            ModelId = modelid;
            DrawDistance = drawDistance;

            Id = Native.CreatePlayerObject(owner.Id, modelid, position.X, position.Y, position.Z, rotation.X, rotation.Y,
                rotation.Z, drawDistance);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Moves this <see cref="IGameObject" /> to the given position and rotation with the given speed.
        /// </summary>
        /// <param name="position">The position to which to move this <see cref="IGameObject" />.</param>
        /// <param name="speed">The speed at which to move this <see cref="IGameObject" />.</param>
        /// <param name="rotation">The rotation to which to move this <see cref="IGameObject" />.</param>
        /// <returns>
        /// The time it will take for the object to move in milliseconds.
        /// </returns>
        public virtual int Move(Vector position, float speed, Vector rotation)
        {
            AssertNotDisposed();

            return Native.MovePlayerObject(Owner.Id, Id, position.X, position.Y, position.Z, speed, rotation.X,
                rotation.Y, rotation.Z);
        }

        /// <summary>
        /// Moves this <see cref="IGameObject" /> to the given position with the given speed.
        /// </summary>
        /// <param name="position">The position to which to move this <see cref="IGameObject" />.</param>
        /// <param name="speed">The speed at which to move this <see cref="IGameObject" />.</param>
        /// <returns>
        /// The time it will take for the object to move in milliseconds.
        /// </returns>
        public virtual int Move(Vector position, float speed)
        {
            AssertNotDisposed();

            return Native.MovePlayerObject(Owner.Id, Id, position.X, position.Y, position.Z, speed, -1000,
                -1000, -1000);
        }

        /// <summary>
        /// Stop this <see cref="IGameObject" /> from moving any further.
        /// </summary>
        public virtual void Stop()
        {
            AssertNotDisposed();

            Native.StopPlayerObject(Owner.Id, Id);
        }

        /// <summary>
        /// Sets the material of this <see cref="IGameObject" />.
        /// </summary>
        /// <param name="materialindex">The material index on the object to change.</param>
        /// <param name="modelid">The modelid on which the replacement texture is located. Use 0 for alpha. Use -1 to change the
        /// material color without altering the texture.</param>
        /// <param name="txdname">The name of the txd file which contains the replacement texture (use "none" if not required).</param>
        /// <param name="texturename">The name of the texture to use as the replacement (use "none" if not required).</param>
        /// <param name="materialcolor">The object color to set (use default(Color) to keep the existing material color).</param>
        public virtual void SetMaterial(int materialindex, int modelid, string txdname, string texturename,
            Color materialcolor)
        {
            AssertNotDisposed();

            Native.SetPlayerObjectMaterial(Owner.Id, Id, materialindex, modelid, txdname, texturename,
                materialcolor.ToInteger(ColorFormat.ARGB));
        }

        /// <summary>
        /// Sets the material text of this <see cref="IGameObject" />.
        /// </summary>
        /// <param name="materialindex">The material index on the object to change.</param>
        /// <param name="text">The text to show on the object. (MAX 2048 characters)</param>
        /// <param name="materialsize">The object's material index to replace with text.</param>
        /// <param name="fontface">The font to use.</param>
        /// <param name="fontsize">The size of the text (max 255).</param>
        /// <param name="bold">Whether to write in bold.</param>
        /// <param name="foreColor">The color of the text.</param>
        /// <param name="backColor">The background color of the text.</param>
        /// <param name="textalignment">The alignment of the text.</param>
        public virtual void SetMaterialText(int materialindex, string text, ObjectMaterialSize materialsize,
            string fontface, int fontsize, bool bold, Color foreColor, Color backColor,
            ObjectMaterialTextAlign textalignment)
        {
            AssertNotDisposed();

            Native.SetPlayerObjectMaterialText(Owner.Id, Id, text, materialindex, (int) materialsize,
                fontface, fontsize, bold,
                foreColor.ToInteger(ColorFormat.ARGB), backColor.ToInteger(ColorFormat.ARGB),
                (int) textalignment);
        }

        /// <summary>
        ///     Attaches this <see cref="PlayerObject" /> to the specified <paramref name="player" />.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="rotation">The rotation.</param>
        /// <exception cref="System.ArgumentNullException">player</exception>
        public virtual void AttachTo(GtaPlayer player, Vector offset, Vector rotation)
        {
            AssertNotDisposed();

            if (player == null)
                throw new ArgumentNullException("player");

            Native.AttachPlayerObjectToPlayer(Owner.Id, Id, player.Id, offset.X, offset.Y, offset.Z, rotation.X,
                rotation.Y, rotation.Z);
        }

        /// <summary>
        ///     Attaches this <see cref="PlayerObject" /> to the specified <paramref name="vehicle" />.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="rotation">The rotation.</param>
        /// <exception cref="System.ArgumentNullException">vehicle</exception>
        public virtual void AttachTo(GtaVehicle vehicle, Vector offset, Vector rotation)
        {
            AssertNotDisposed();

            if (vehicle == null)
                throw new ArgumentNullException("vehicle");

            Native.AttachPlayerObjectToVehicle(Owner.Id, Id, vehicle.Id, offset.X, offset.Y, offset.Z, rotation.X,
                rotation.Y, rotation.Z);
        }

        /// <summary>
        ///     Attaches the player's camera to this <see cref="PlayerObject" />.
        /// </summary>
        /// <remarks>
        ///     This will attach the camera of the player whose object this is to this object.
        /// </remarks>
        public virtual void AttachCameraToObject()
        {
            AssertNotDisposed();

            Native.AttachCameraToPlayerObject(Owner.Id, Id);
        }

        /// <summary>
        ///     Performs tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">Whether managed resources should be disposed.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            Native.DestroyPlayerObject(Owner.Id, Id);
        }

        /// <summary>
        ///     Lets the <see cref="Owner" /> of this <see cref="PlayerObject" /> edit this object.
        /// </summary>
        public virtual void Edit()
        {
            AssertNotDisposed();

            Native.EditPlayerObject(Owner.Id, Id);
        }

        /// <summary>
        ///     Lets the specified <paramref name="player" /> select an object.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <exception cref="System.ArgumentNullException">player</exception>
        public static void Select(GtaPlayer player)
        {
            if (player == null)
                throw new ArgumentNullException("player");

            Native.SelectObject(player.Id);
        }

        #endregion

        #region Event raisers

        /// <summary>
        ///     Raises the <see cref="Moved" /> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs" /> that contains the event data. </param>
        public virtual void OnMoved(EventArgs e)
        {
            if (Moved != null)
                Moved(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="Selected" /> event.
        /// </summary>
        /// <param name="e">An <see cref="SelectGlobalObjectEventArgs" /> that contains the event data. </param>
        public virtual void OnSelected(SelectPlayerObjectEventArgs e)
        {
            if (Selected != null)
                Selected(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="Edited" /> event.
        /// </summary>
        /// <param name="e">An <see cref="EditPlayerObjectEventArgs" /> that contains the event data. </param>
        public virtual void OnEdited(EditPlayerObjectEventArgs e)
        {
            if (Edited != null)
                Edited(this, e);
        }

        #endregion
    }
}