// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Threading;

namespace x2
{
    /// <summary>
    /// Provides a disposalbe read lock.
    /// </summary>
    public class ReadLock : IDisposable
    {
        private readonly ReaderWriterLockSlim rwlock;

        /// <summary>
        /// Initializes a new instance of the ReadLock class to acquire a read
        /// lock based on the specified ReaderWriterLockSlim object.
        /// </summary>
        public ReadLock(ReaderWriterLockSlim rwlock)
        {
            this.rwlock = rwlock;
            rwlock.EnterReadLock();
        }

        /// <summary>
        /// Releases the read lock held by this object.
        /// </summary>
        public void Dispose()
        {
            rwlock.ExitReadLock();
        }
    }

    /// <summary>
    /// Provides a disposalbe write lock.
    /// </summary>
    public class WriteLock : IDisposable
    {
        private readonly ReaderWriterLockSlim rwlock;

        /// <summary>
        /// Initializes a new instance of the WriteLock class to acquire a write
        /// lock based on the specified ReaderWriterLockSlim object.
        /// </summary>
        public WriteLock(ReaderWriterLockSlim rwlock)
        {
            this.rwlock = rwlock;
            rwlock.EnterWriteLock();
        }

        /// <summary>
        /// Releases the write lock held by this object.
        /// </summary>
        public void Dispose()
        {
            rwlock.ExitWriteLock();
        }
    }
}
