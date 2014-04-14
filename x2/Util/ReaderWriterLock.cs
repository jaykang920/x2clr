// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Threading;

namespace x2
{
    public class ReadLockSlim : IDisposable
    {
        private readonly ReaderWriterLockSlim rwlock;

        public ReadLockSlim(ReaderWriterLockSlim rwlock)
        {
            this.rwlock = rwlock;
            rwlock.EnterReadLock();
        }

        public void Dispose()
        {
            rwlock.ExitReadLock();
        }
    }

    public class WriteLockSlim : IDisposable
    {
        private readonly ReaderWriterLockSlim rwlock;

        public WriteLockSlim(ReaderWriterLockSlim rwlock)
        {
            this.rwlock = rwlock;
            rwlock.EnterWriteLock();
        }

        public void Dispose()
        {
            rwlock.ExitWriteLock();
        }
    }
}
