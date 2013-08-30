// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2
{
    public sealed class Hub
    {
        private const string defaultName = "anonymous";
        private static readonly HubMap hubMap = new HubMap();
        private readonly FlowSet flowSet = new FlowSet();
        private readonly string name;

        /// <summary>
        ///   Gets the name of the hub.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        static Hub()
        {
            // Create the default(anonymous) hub.
            Create();
        }

        private Hub(string name)
        {
            this.name = name;
        }

        /// <summary>
        ///   Creates a default(anonymous) hub.
        /// </summary>
        public static Hub Create()
        {
            return Create(defaultName);
        }

        /// <summary>
        ///   Creates a named hub.
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
        ///   Gets the default(anonymous) hub.
        /// </summary>
        public static Hub Get()
        {
            return Get(defaultName);
        }

        /// <summary>
        ///   Gets the named hub.
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
            private readonly ReaderWriterLock rwlock = new ReaderWriterLock();

            internal Hub Get(string name)
            {
                rwlock.AcquireReaderLock(Timeout.Infinite);
                try
                {
                    Hub hub;
                    return hubs.TryGetValue(name, out hub) ? hub : null;
                }
                finally
                {
                    rwlock.ReleaseReaderLock();
                }
            }

            internal Hub Create(string name)
            {
                rwlock.AcquireWriterLock(Timeout.Infinite);
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
                    rwlock.ReleaseWriterLock();
                }
            }

            internal void StartAllFlows()
            {
                rwlock.AcquireReaderLock(Timeout.Infinite);
                try
                {
                    foreach (Hub hub in hubs.Values)
                    {
                        hub.StartAttachedFlows();
                    }
                }
                finally
                {
                    rwlock.ReleaseReaderLock();
                }
            }

            internal void StopAllFlows()
            {
                rwlock.AcquireReaderLock(Timeout.Infinite);
                try
                {
                    foreach (Hub hub in hubs.Values)
                    {
                        hub.StopAttachedFlows();
                    }
                }
                finally
                {
                    rwlock.ReleaseReaderLock();
                }
            }
        }

        private class FlowSet
        {
            private readonly List<Flow> flows = new List<Flow>();
            private readonly ReaderWriterLock rwlock = new ReaderWriterLock();

            internal bool Add(Flow flow)
            {
                rwlock.AcquireWriterLock(Timeout.Infinite);
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
                    rwlock.ReleaseWriterLock();
                }
            }

            internal void Feed(Event e)
            {
                rwlock.AcquireReaderLock(Timeout.Infinite);
                try
                {
                    foreach (Flow flow in flows)
                    {
                        flow.Feed(e);
                    }
                }
                finally
                {
                    rwlock.ReleaseReaderLock();
                }
            }

            internal void Feed(Event e, Flow except)
            {
                rwlock.AcquireReaderLock(Timeout.Infinite);
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
                    rwlock.ReleaseReaderLock();
                }
            }

            internal bool Remove(Flow flow)
            {
                rwlock.AcquireWriterLock(Timeout.Infinite);
                try
                {
                    return flows.Remove(flow);
                }
                finally
                {
                    rwlock.ReleaseWriterLock();
                }
            }

            internal void StartAll()
            {
                rwlock.AcquireReaderLock(Timeout.Infinite);
                try
                {
                    foreach (Flow flow in flows)
                    {
                        flow.StartUp();
                    }
                }
                finally
                {
                    rwlock.ReleaseReaderLock();
                }
            }

            internal void StopAll()
            {
                rwlock.AcquireReaderLock(Timeout.Infinite);
                try
                {
                    foreach (Flow flow in flows)
                    {
                        flow.ShutDown();
                    }
                }
                finally
                {
                    rwlock.ReleaseReaderLock();
                }
            }
        }
    }
}
