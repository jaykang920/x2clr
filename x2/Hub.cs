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
        // List of all the flows attached to the hub.
        private static readonly List<Flow> flows;
        // Explicit channel subscription map.
        private static readonly Dictionary<string, List<Flow>> subscriptions;

        private static readonly ReaderWriterLockSlim rwlock;

        private static readonly Hub instance;

        /// <summary>
        /// Gets the singleton instance of the hub.
        /// </summary>
        public static Hub Instance { get { return instance; } }

        static Hub()
        {
            flows = new List<Flow>();
            subscriptions = new Dictionary<string, List<Flow>>();

            rwlock = new ReaderWriterLockSlim();

            instance = new Hub();
        }

        // Private constructor to prevent explicit instantiation.
        private Hub()
        {
        }

        /// <summary>
        /// Attaches the specified flow to the hub.
        /// </summary>
        public Hub Attach(Flow flow)
        {
            if (Object.ReferenceEquals(flow, null))
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
            if (Object.ReferenceEquals(flow, null))
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
            if (Object.ReferenceEquals(e, null))
            {
                throw new NullReferenceException();
            }
            rwlock.EnterReadLock();  // not using ReadLockSlim intentionally
            try
            {
                string channel = e._Channel;
                if (String.IsNullOrEmpty(channel))
                {
                    // Distribute the event throughout all the attached flows.
                    for (int i = 0, count = flows.Count; i < count; ++i)
                    {
                        flows[i].Feed(e);
                    }
                }
                else
                {
                    // Or pass it to the explicit subscribers.
                    List<Flow> subscribers;
                    if (subscriptions.TryGetValue(channel, out subscribers))
                    {
                        for (int i = 0, count = subscribers.Count; i < count; ++i)
                        {
                            subscribers[i].Feed(e);
                        }
                    }
                }
            }
            finally
            {
                rwlock.ExitReadLock();
            }
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
        /// Makes the given flow subscribe to the specifieid channel.
        /// </summary>
        internal static void Subscribe(Flow flow, string channel)
        {
            if (String.IsNullOrEmpty(channel))
            {
                throw new ArgumentException();
            }
            using (new WriteLockSlim(rwlock))
            {
                List<Flow> subscribers;
                if (subscriptions.TryGetValue(channel, out subscribers))
                {
                    if (subscribers.Contains(flow))
                    {
                        return;
                    }
                }
                else
                {
                    subscribers = new List<Flow>();
                    subscriptions.Add(channel, subscribers);
                }
                subscribers.Add(flow);
            }
        }

        /// <summary>
        /// Makes the given flow unsubscribe from the specified channel.
        /// </summary>
        internal static void Unsubscribe(Flow flow, string channel)
        {
            if (String.IsNullOrEmpty(channel))
            {
                throw new ArgumentException();
            }
            using (new WriteLockSlim(rwlock))
            {
                List<Flow> subscribers;
                if (!subscriptions.TryGetValue(channel, out subscribers))
                {
                    return;
                }
                if (subscribers.Remove(flow))
                {
                    if (subscribers.Count == 0)
                    {
                        subscriptions.Remove(channel);
                    }
                }
            }
        }
    }
}
