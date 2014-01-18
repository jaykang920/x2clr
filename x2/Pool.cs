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

        public int Capacity { get { return capacity; } }
        public int Count { get { return store.Count; } }

        public Pool()
        {
            store = new Stack<T>();
        }

        public Pool(int capacity)
        {
            store = new Stack<T>(capacity);
            this.capacity = capacity;
        }

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
