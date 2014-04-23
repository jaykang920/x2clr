// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2
{
    /// <summary>
    /// Represents the singleton event distribution bus.
    /// </summary>
    public sealed class Hub
    {
        private static readonly IList<Flow> flows;

        // TODO: channel subscription

        private static readonly ReaderWriterLockSlim rwlock;

        private static readonly Hub instance;

        /// <summary>
        /// Gets the singleton instance of the hub.
        /// </summary>
        public static Hub Instance { get { return instance; } }

        static Hub()
        {
            flows = new List<Flow>();
            rwlock = new ReaderWriterLockSlim();

            instance = new Hub();
        }

        private Hub()
        {
        }

        /// <summary>
        /// Starts all the flows attached to the hub.
        /// </summary>
        public static void StartAllFlows()
        {
            using (new ReadLockSlim(rwlock))
            {
                for (int i = 0, count = flows.Count; i < count; ++i)
                {
                    flows[i].StartUp();
                }
            }
        }

        /// <summary>
        /// Stops all the flows attached to the hub.
        /// </summary>
        public static void StopAllFlows()
        {
            using (new ReadLockSlim(rwlock))
            {
                for (int i = 0, count = flows.Count; i < count; ++i)
                {
                    flows[i].ShutDown();
                }
            }
        }

        /// <summary>
        /// Attaches the specified flow to the hub.
        /// </summary>
        public Hub Attach(Flow flow)
        {
            if (flow == null)
            {
                throw new NullReferenceException();
            }
            using (new WriteLockSlim(rwlock))
            {
                if (!flows.Contains(flow))
                {
                    flows.Add(flow);
                }
            }
            return this;
        }

        /// <summary>
        /// Detaches the specified flow from the hub.
        /// </summary>
        public Hub Detach(Flow flow)
        {
            if (flow == null)
            {
                throw new NullReferenceException();
            }
            using (new WriteLockSlim(rwlock))
            {
                flows.Remove(flow);
            }
            return this;
        }

        /// <summary>
        /// Posts up the specified event to the hub.
        /// </summary>
        public static void Post(Event e)
        {
            if (e == null)
            {
                throw new NullReferenceException();
            }
            rwlock.EnterReadLock();
            try
            {
                for (int i = 0, count = flows.Count; i < count; ++i)
                {
                    flows[i].Feed(e);
                }
            }
            finally
            {
                rwlock.ExitReadLock();
            }
        }
    }
}
