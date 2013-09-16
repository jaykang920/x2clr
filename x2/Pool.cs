// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2
{
    /// <summary>
    /// Common base class for any pooled object with custom lifecycle based on
    /// reference counting.
    /// </summary>
    public abstract class PooledObject
    {
        private int refCount;

        /// <summary>
        /// Releases this object and returns it to the pool.
        /// </summary>
        public abstract void Release();

        /// <summary>
        /// Increases the reference count to this object.
        /// </summary>
        public void AcquireReference()
        {
            Interlocked.Increment(ref refCount);
        }

        /// <summary>
        /// Decreases the reference count to this object, and releases this if
        /// it reached zero.
        /// </summary>
        public void ReleaseReference()
        {
            if (Interlocked.Decrement(ref refCount) == 0)
            {
                Release();
            }
        }
    }

    /// <summary>
    /// Minimal generic object pool.
    /// </summary>
    /// <typeparam name="T">
    /// Specifies the type of objects in the pool.
    /// </typeparam>
    public class Pool<T> where T : class
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
            return null;
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
        /// Gets the diagnostics object.
        /// </summary>
        public Diagnostics Diag { get; private set; }

        /// <summary>
        /// Internal diagnostics helper class.
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
