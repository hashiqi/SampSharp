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
using System.Reflection;
using SampSharp.GameMode.Controllers;

namespace SampSharp.GameMode
{
    /// <summary>
    ///     Base class for a SA-MP game mode.
    /// </summary>
    public abstract partial class BaseMode : IDisposable
    {
        private readonly ControllerCollection _controllers = new ControllerCollection();

        #region Constructor

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseMode" /> class.
        /// </summary>
        protected BaseMode()
        {
            Console.SetOut(new LogWriter());

            Type type = Type.GetType("Mono.Runtime");
            if (type != null)
            {
                MethodInfo displayName = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
                if (displayName != null)
                    Console.WriteLine("Detected mono version: {0}", displayName.Invoke(null, null));
            }

            RegisterControllers();
        }

        #endregion

        /// <summary>
        ///     Gets the collection of controllers loaded.
        /// </summary>
        protected virtual ControllerCollection Controllers
        {
            get { return _controllers; }
        }

        #region Methods

        private void RegisterControllers()
        {
            LoadControllers(_controllers);

            foreach (IController controller in _controllers)
            {
                var typeProvider = controller as ITypeProvider;
                var eventListener = controller as IEventListener;

                if (typeProvider != null)
                    typeProvider.RegisterTypes();

                if (eventListener != null)
                    eventListener.RegisterEvents(this);
            }
        }

        /// <summary>
        ///     Loads all default controllers into the given ControllerCollection.
        /// </summary>
        /// <param name="controllers">The collection to load the default controllers into.</param>
        protected virtual void LoadControllers(ControllerCollection controllers)
        {
            controllers.Add(new CommandController());
            controllers.Add(new DialogController());
            controllers.Add(new GlobalObjectController());
            controllers.Add(new MenuController());
            controllers.Add(new GtaPlayerController());
            controllers.Add(new PlayerObjectController());
            controllers.Add(new PlayerTextDrawController());
            controllers.Add(new TextDrawController());
            controllers.Add(new TimerController());
            controllers.Add(new DelayController());
            controllers.Add(new GtaVehicleController());
            controllers.Add(new SyncController());
            controllers.Add(new PickupController());
        }

        #endregion

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _controllers.Dispose();

            GC.Collect();
            GC.WaitForPendingFinalizers(); 
        }
    }
}