// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;

namespace x2
{
    /// <summary>
    /// Simple grow-only generic pool.
    /// </summary>
    /// <typeparam name="T">
    /// Specifies the type of elements in the pool.
    /// </typeparam>
    public class Pool<T>
        where T : new()
    {
        private readonly Stack<T> store = new Stack<T>();

        public T Acquire()
        {
            lock (store)
            {
                if (store.Count != 0)
                {
                    return store.Pop();
                }
            }
            return new T();
        }

        public void Release(T item)
        {
            lock (store)
            {
                store.Push(item);
            }
        }
    }
}
