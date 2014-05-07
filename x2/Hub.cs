// Copyright (c) 2013, 2014 Jae-jun Kang
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
        // List of all the flows attached to the hub
        private static readonly List<Flow> attached;

        // List of the flows which is subscribed to the default(unnamed) channel
        private static readonly List<Flow> defaultSubscribers;
        // Explicit channel subscription map
        private static readonly Dictionary<string, List<Flow>> subscriptions;

        private static readonly ReaderWriterLockSlim rwlock;

        private static readonly Hub instance;

        /// <summary>
        /// Gets the singleton instance of the hub.
        /// </summary>
        public static Hub Instance { get { return instance; } }

        static Hub()
        {
            attached = new List<Flow>();

            defaultSubscribers = new List<Flow>();
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
            using (new WriteLock(rwlock))
            {
                if (!attached.Contains(flow))
                {
                    attached.Add(flow);
                    SubscribeInternal(flow, null);
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
            using (new WriteLock(rwlock))
            {
                if (attached.Remove(flow))
                {
                    UnsubscribeInternal(flow);
                }
            }
            return this;
        }

        /// <summary>
        /// Detaches all the attached flows.
        /// </summary>
        public static void DetachAll()
        {
            using (new WriteLock(rwlock))
            {
                for (int i = 0, count = attached.Count; i < count; ++i)
                {
                    UnsubscribeInternal(attached[i]);
                }
                attached.Clear();
            }
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

            rwlock.EnterReadLock();  // not using ReadLock intentionally
            try
            {
                List<Flow> subscribers;
                string channel = e._Channel;

                if (String.IsNullOrEmpty(channel))
                {
                    subscribers = defaultSubscribers;
                }
                else
                {
                    if (!subscriptions.TryGetValue(channel, out subscribers))
                    {
                        return;
                    }
                }
                for (int i = 0, count = subscribers.Count; i < count; ++i)
                {
                    subscribers[i].Feed(e);
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
        public static void StartAttachedFlows()
        {
            using (new ReadLock(rwlock))
            {
                for (int i = 0, count = attached.Count; i < count; ++i)
                {
                    attached[i].StartUp();
                }
            }
        }

        /// <summary>
        /// Stops all the flows attached to the hub.
        /// </summary>
        public static void StopAttachedFlows()
        {
            using (new ReadLock(rwlock))
            {
                for (int i = 0, count = attached.Count; i < count; ++i)
                {
                    attached[i].ShutDown();
                }
            }
        }

        /// <summary>
        /// Makes the given flow subscribe to the specifieid channel.
        /// </summary>
        internal static void Subscribe(Flow flow, string channel)
        {
            if (Object.ReferenceEquals(flow, null))
            {
                throw new NullReferenceException();
            }
            using (new WriteLock(rwlock))
            {
                if (!attached.Contains(flow))
                {
                    throw new InvalidOperationException();
                }
                SubscribeInternal(flow, channel);
            }
        }

        /// <summary>
        /// Makes the given flow unsubscribe from the specified channel.
        /// </summary>
        internal static void Unsubscribe(Flow flow, string channel)
        {
            if (Object.ReferenceEquals(flow, null))
            {
                throw new NullReferenceException();
            }
            using (new WriteLock(rwlock))
            {
                if (!attached.Contains(flow))
                {
                    throw new InvalidOperationException();
                }
                UnsubscribeInternal(flow, channel);
            }
        }

        // Lets the given flow subscribe to the specified channel.
        private static void SubscribeInternal(Flow flow, string channel)
        {
            List<Flow> subscribers;
            if (String.IsNullOrEmpty(channel))
            {
                if (defaultSubscribers.Contains(flow))
                {
                    return;
                }
                subscribers = defaultSubscribers;
            }
            else
            {
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
            }
            subscribers.Add(flow);
        }

        // Lets the given flow unsubscribe from the specified channel.
        private static void UnsubscribeInternal(Flow flow, string channel)
        {
            if (String.IsNullOrEmpty(channel))
            {
                defaultSubscribers.Remove(flow);
            }
            else
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

        // Lets the given flow unsubscribe from all the channels.
        private static void UnsubscribeInternal(Flow flow)
        {
            defaultSubscribers.Remove(flow);

            var keysToRemove = new List<string>();

            foreach (var pair in subscriptions)
            {
                var subscribers = pair.Value;
                if (subscribers.Remove(flow))
                {
                    if (subscribers.Count == 0)
                    {
                        keysToRemove.Add(pair.Key);
                    }
                }
            }

            foreach (var key in keysToRemove)
            {
                subscriptions.Remove(key);
            }
        }

        /// <summary>
        /// Represents the attached flows for convenience.
        /// </summary>
        public sealed class Flows : IDisposable
        {
            /// <summary>
            /// Detaches all the attached flows.
            /// </summary>
            public void Detach()
            {
                DetachAll();
            }

            /// <summary>
            /// Implements the IDisposable interface.
            /// </summary>
            public void Dispose()
            {
                Stop();
            }

            /// <summary>
            /// Starts all the attached flows.
            /// </summary>
            public void Start()
            {
                StartAttachedFlows();
            }

            /// <summary>
            /// Stops all the attached flows.
            /// </summary>
            public void Stop()
            {
                StopAttachedFlows();
            }
        }
    }
}
