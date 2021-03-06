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
using System.Collections.ObjectModel;
using System.Linq;
using SampSharp.GameMode.Tools;

namespace SampSharp.GameMode.Pools
{
    /// <summary>
    ///     Keeps track of a pool of instances.
    /// </summary>
    /// <typeparam name="T">Base type of instances to keep track of.</typeparam>
    public abstract class Pool<T> : Disposable
    {
        /// <summary>
        ///     The instances alive in this pool.
        /// </summary>
        protected static readonly List<object> Instances = new List<object>();

        /// <summary>
        ///     A readonly collection of the instances in this pool.
        /// </summary>
        protected static ReadOnlyCollection<T> ReadOnly = new ReadOnlyCollection<T>(new List<T>());

        /// <summary>
        ///     A Locker for tread-saving this pool.
        /// </summary>
        protected static object Lock = new object();

        /// <summary>
        ///     Initializes a new instance of the <see cref="Pool{T}" /> class.
        /// </summary>
        protected Pool()
        {
            lock (Lock)
            {
                Instances.Add(this);
                ReadOnly = Instances.OfType<T>().ToList().AsReadOnly();
            }
        }

        /// <summary>
        ///     Gets a <see cref="ReadOnlyCollection{T}" /> containing all instances of type.
        /// </summary>
        public static ReadOnlyCollection<T> All
        {
            get
            {
                lock (Lock)
                {
                    return ReadOnly;
                }
            }
        }

        /// <summary>
        ///     Removes this instance from the pool.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            lock (Lock)
            {
                Instances.Remove(this);
                ReadOnly = Instances.OfType<T>().ToList().AsReadOnly();
            }
        }

        /// <summary>
        ///     Gets whether the given instance is present in the pool.
        /// </summary>
        /// <param name="item">The instance to check the presence of.</param>
        /// <returns>Whether the given instance is present in the pool.</returns>
        public static bool Contains(T item)
        {
            lock (Lock)
            {
                return Instances.Contains(item);
            }
        }

        /// <summary>
        ///     Gets a <see cref="ReadOnlyCollection{T}" /> containing all instances of the given type within this <see cref="Pool{T}" />.
        /// </summary>
        /// <typeparam name="T2">The <see cref="Type" /> of instances to get.</typeparam>
        /// <returns>All instances of the given type within this <see cref="Pool{T}" />.</returns>
        public static ReadOnlyCollection<T2> GetAll<T2>()
        {
            lock (Lock)
            {
                return Instances.OfType<T2>().ToList().AsReadOnly();
            }
        }
    }
}