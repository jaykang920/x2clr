// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2
{
    /// <summary>
    /// Minimal generic object pool.
    /// </summary>
    /// <typeparam name="T">
    /// The type of objects in the pool.
    /// </typeparam>
    public class Pool<T> where T : class
    {
        private readonly Stack<T> store;
        private readonly int capacity;

        /// <summary>
        /// Gets the maximum number of objects that can be contained in the pool.
        /// </summary>
        public int Capacity { get { return capacity; } }

        /// <summary>
        /// Gets the number of objects contained in the pool.
        /// </summary>
        public int Count { get { return store.Count; } }

        /// <summary>
        /// Initializes a new instance of the Pool(T) class, without a capacity
        /// limit.
        /// </summary>
        public Pool()
        {
            store = new Stack<T>();
        }

        /// <summary>
        /// Initializes a new instance of the Pool(T) class, with the specified
        /// maximum capacity.
        /// </summary>
        /// <param name="capacity"></param>
        public Pool(int capacity)
        {
            store = new Stack<T>(capacity);
            this.capacity = capacity;
        }

        /// <summary>
        /// Tries to pop an object out of the pool.
        /// </summary>
        /// <returns>
        /// The object removed from the pool, or null if the pool is empty.
        /// </returns>
        public T Pop()
        {
            lock (store)
            {
                if (store.Count != 0)
                {
                    return store.Pop();
                }
            }
            return null;
        }

        /// <summary>
        /// Tries to push the specified object into the pool.
        /// </summary>
        /// <remarks>
        /// If the pool has a non-zero capacity limit, the object may be dropped
        /// when the number of pooled objects reaches the capacity.
        /// </remarks>
        public void Push(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException();
            }

            lock (store)
            {
                if (capacity == 0 || store.Count < capacity)
                {
                    store.Push(item);
                }
            }
        }
    }
}
