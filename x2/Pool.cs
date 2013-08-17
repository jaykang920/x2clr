// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Threading;

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
        private readonly Stack<T> store;

        public Pool()
        {
            store = new Stack<T>();
            Diag = new Diagnostics(this);
        }

        public T Acquire()
        {
            Diag.IncrementAcquireCount();

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
            Diag.IncrementReleaseCount();

            lock (store)
            {
                store.Push(item);
            }
        }

        #region Diagnostics

        /// <summary>
        /// Gets the Diagnostics object for this Pool.
        /// </summary>
        public Diagnostics Diag { get; private set; }

        /// <summary>
        /// Internal diagnostics helper class for Pool.
        /// </summary>
        public class Diagnostics
        {
            private readonly Pool<T> owner;

            private int acquireCount;
            private int releaseCount;

            public int AcquireCount { get { return acquireCount; } }
            public int ReleaseCount { get { return releaseCount; } }

            public int StoreSize {
                get
                {
                    lock (owner.store)
                    {
                        return owner.store.Count;
                    }
                }
            }

            internal Diagnostics(Pool<T> owner)
            {
                this.owner = owner;
            }

            public void IncrementAcquireCount()
            {
                Interlocked.Increment(ref acquireCount);
            }

            public void IncrementReleaseCount()
            {
                Interlocked.Increment(ref releaseCount);
            }
        }

        #endregion
    }
}
