﻿// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2
{
    public sealed class Hub
    {
        private const string defaultName = "default";

        private static readonly HubMap hubMap;
        private readonly FlowSet flowSet;
        private readonly string name;

        /// <summary>
        /// Gets the name of this hub.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        static Hub()
        {
            hubMap = new HubMap();
            // Create the default(anonymous) hub.
            Create();
        }

        private Hub(string name)
        {
            flowSet = new FlowSet();
            this.name = name;
        }

        /// <summary>
        /// Creates a default(anonymous) hub.
        /// </summary>
        public static Hub Create()
        {
            return Create(defaultName);
        }

        /// <summary>
        /// Creates a named hub.
        /// </summary>
        public static Hub Create(string name)
        {
            if (name == null)
            {
                throw new NullReferenceException();
            }
            return hubMap.Create(name);
        }

        /// <summary>
        /// Gets the default(anonymous) hub.
        /// </summary>
        public static Hub Get()
        {
            return Get(defaultName);
        }

        /// <summary>
        /// Gets the named hub.
        /// </summary>
        public static Hub Get(string name)
        {
            if (name == null)
            {
                throw new NullReferenceException();
            }
            return hubMap.Get(name);
        }

        /// <summary>
        /// Starts all the flows attached to the hubs in the current process.
        /// </summary>
        public static void StartAllFlows()
        {
            hubMap.StartAllFlows();
        }

        /// <summary>
        /// Stops all the flows attached to the hubs in the current process.
        /// </summary>
        public static void StopAllFlows()
        {
            hubMap.StopAllFlows();
        }

        /// <summary>
        /// Attaches the specified flow to this hub.
        /// </summary>
        public Hub Attach(Flow flow)
        {
            flow.AttachTo(this);
            return this;
        }

        /// <summary>
        /// Detaches the specified flow from this hub.
        /// </summary>
        public Hub Detach(Flow flow)
        {
            flow.DetachFrom(this);
            return this;
        }

        internal bool AttachInternal(Flow flow)
        {
            if (flow == null)
            {
                throw new NullReferenceException();
            }
            return flowSet.Add(flow);
        }

        internal bool DetachInternal(Flow flow)
        {
            if (flow == null)
            {
                throw new NullReferenceException();
            }
            return flowSet.Remove(flow);
        }

        public void Post(Event e)
        {
            if (e == null)
            {
                throw new NullReferenceException();
            }
            flowSet.Feed(e);
        }

        public void Post(Event e, Flow except)
        {
            if (e == null)
            {
                throw new NullReferenceException();
            }
            flowSet.Feed(e, except);
        }

        private void StartAttachedFlows()
        {
            flowSet.StartAll();
        }

        private void StopAttachedFlows()
        {
            flowSet.StopAll();
        }

        private class HubMap
        {
            private readonly Dictionary<string, Hub> hubs = new Dictionary<string, Hub>();
            private readonly ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim();

            internal Hub Get(string name)
            {
                rwlock.EnterReadLock();
                try
                {
                    Hub hub;
                    return hubs.TryGetValue(name, out hub) ? hub : null;
                }
                finally
                {
                    rwlock.ExitReadLock();
                }
            }

            internal Hub Create(string name)
            {
                rwlock.EnterWriteLock();
                try
                {
                    Hub hub;
                    if (!hubs.TryGetValue(name, out hub))
                    {
                        hub = new Hub(name);
                        hubs.Add(name, hub);
                    }
                    return hub;
                }
                finally
                {
                    rwlock.ExitWriteLock();
                }
            }

            internal void StartAllFlows()
            {
                rwlock.EnterReadLock();
                try
                {
                    foreach (Hub hub in hubs.Values)
                    {
                        hub.StartAttachedFlows();
                    }
                }
                finally
                {
                    rwlock.ExitReadLock();
                }
            }

            internal void StopAllFlows()
            {
                rwlock.EnterReadLock();
                try
                {
                    foreach (Hub hub in hubs.Values)
                    {
                        hub.StopAttachedFlows();
                    }
                }
                finally
                {
                    rwlock.ExitReadLock();
                }
            }
        }

        private class FlowSet
        {
            private readonly List<Flow> flows = new List<Flow>();
            private readonly ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim();

            internal bool Add(Flow flow)
            {
                rwlock.EnterWriteLock();
                try
                {
                    if (flows.Contains(flow))
                    {
                        return false;
                    }
                    flows.Add(flow);
                    return true;
                }
                finally
                {
                    rwlock.ExitWriteLock();
                }
            }

            internal void Feed(Event e)
            {
                rwlock.EnterReadLock();
                try
                {
                    foreach (Flow flow in flows)
                    {
                        flow.Feed(e);
                    }
                }
                finally
                {
                    rwlock.ExitReadLock();
                }
            }

            internal void Feed(Event e, Flow except)
            {
                rwlock.EnterReadLock();
                try
                {
                    foreach (Flow flow in flows)
                    {
                        if (Object.ReferenceEquals(flow, except))
                        {
                            continue;
                        }
                        flow.Feed(e);
                    }
                }
                finally
                {
                    rwlock.ExitReadLock();
                }
            }

            internal bool Remove(Flow flow)
            {
                rwlock.EnterWriteLock();
                try
                {
                    return flows.Remove(flow);
                }
                finally
                {
                    rwlock.ExitWriteLock();
                }
            }

            internal void StartAll()
            {
                rwlock.EnterReadLock();
                try
                {
                    foreach (Flow flow in flows)
                    {
                        flow.StartUp();
                    }
                }
                finally
                {
                    rwlock.ExitReadLock();
                }
            }

            internal void StopAll()
            {
                rwlock.EnterReadLock();
                try
                {
                    foreach (Flow flow in flows)
                    {
                        flow.ShutDown();
                    }
                }
                finally
                {
                    rwlock.ExitReadLock();
                }
            }
        }
    }
}
