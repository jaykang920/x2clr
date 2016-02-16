﻿// Copyright (c) 2013-2016 Jae-jun Kang
// See the file LICENSE for details.

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
        private List<Flow> flows;
        // Explicit (named) channel subscription map
        private Dictionary<string, List<Flow>> subscriptions;

        private ReaderWriterLockSlim rwlock;

        public static HeartbeatEvent HeartbeatEvent { get; private set; }
        /// <summary>
        /// Gets the singleton instance of the hub.
        /// </summary>
        public static Hub Instance { get; private set; }

        static Hub()
        {
            HeartbeatEvent = new HeartbeatEvent { _Transform = false };
            Instance = new Hub();
        }

        // Private constructor to prevent explicit instantiation
        private Hub()
        {
            flows = new List<Flow>();
            subscriptions = new Dictionary<string, List<Flow>>();

            rwlock = new ReaderWriterLockSlim();
        }

        ~Hub()
        {
            rwlock.Dispose();
        }

        /// <summary>
        /// Attaches the specified flow to the hub.
        /// </summary>
        public Hub Attach(Flow flow)
        {
            if (Object.ReferenceEquals(flow, null))
            {
                throw new ArgumentNullException();
            }
            using (new WriteLock(rwlock))
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
                throw new ArgumentNullException();
            }
            using (new WriteLock(rwlock))
            {
                if (flows.Remove(flow))
                {
                    UnsubscribeInternal(flow);
                }
            }
            return this;
        }

        /// <summary>
        /// Detaches all the attached flows.
        /// </summary>
        public void DetachAll()
        {
            using (new WriteLock(rwlock))
            {
                subscriptions.Clear();
                flows.Clear();
            }
        }

        private void Feed(Event e)
        {
            if (Object.ReferenceEquals(e, null))
            {
                throw new ArgumentNullException();
            }

            rwlock.EnterReadLock();  // not using ReadLock intentionally
            try
            {
                List<Flow> subscribers;
                string channel = e._Channel;

                if (String.IsNullOrEmpty(channel))
                {
                    subscribers = flows;
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
        /// Posts up the specified event to the hub.
        /// </summary>
        public static void Post(Event e)
        {
            Instance.Feed(e);
        }

        private void StartAttachedFlows()
        {
            List<Flow> snapshot;
            using (new ReadLock(rwlock))
            {
                snapshot = new List<Flow>(flows);
            }
            for (int i = 0, count = snapshot.Count; i < count; ++i)
            {
                snapshot[i].Startup();
            }
        }

        /// <summary>
        /// Starts all the flows attached to the hub.
        /// </summary>
        public static void Startup()
        {
            Instance.StartAttachedFlows();

            TimeFlow.Default.ReserveRepetition(HeartbeatEvent,
                new TimeSpan(0, 0, Config.HeartbeatInterval));
        }

        private void StopAttachedFlows()
        {
            List<Flow> snapshot;
            using (new ReadLock(rwlock))
            {
                snapshot = new List<Flow>(flows);
            }
            for (int i = 0, count = snapshot.Count; i < count; ++i)
            {
                snapshot[i].Shutdown();
            }
        }

        /// <summary>
        /// Stops all the flows attached to the hub.
        /// </summary>
        public static void Shutdown()
        {
            try
            {
                TimeFlow.Default.CancelRepetition(HeartbeatEvent);

                Instance.StopAttachedFlows();
            }
            catch (Exception e) {}
        }

        /// <summary>
        /// Makes the given attached flow subscribe to the specified channel.
        /// </summary>
        internal void Subscribe(Flow flow, string channel)
        {
            if (Object.ReferenceEquals(flow, null))
            {
                throw new ArgumentNullException();
            }
            using (new WriteLock(rwlock))
            {
                if (!flows.Contains(flow))
                {
                    throw new InvalidOperationException();
                }
                SubscribeInternal(flow, channel);
            }
        }

        /// <summary>
        /// Makes the given attached flow unsubscribe from the specified channel.
        /// </summary>
        internal void Unsubscribe(Flow flow, string channel)
        {
            if (Object.ReferenceEquals(flow, null))
            {
                throw new ArgumentNullException();
            }
            using (new WriteLock(rwlock))
            {
                if (!flows.Contains(flow))
                {
                    throw new InvalidOperationException();
                }
                UnsubscribeInternal(flow, channel);
            }
        }

        // Lets the given flow subscribe to the specified channel.
        private void SubscribeInternal(Flow flow, string channel)
        {
            if (String.IsNullOrEmpty(channel))
            {
                // invalid channel name
                return;
            }

            flow.AddChannelRef();

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

        // Lets the given flow unsubscribe from all the channels.
        private void UnsubscribeInternal(Flow flow)
        {
            var keysToRemove = new List<string>();

            foreach (var pair in subscriptions)
            {
                var subscribers = pair.Value;
                if (subscribers.Remove(flow))
                {
                    flow.ResetChannelRef();
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

        // Lets the given flow unsubscribe from the specified channel.
        private void UnsubscribeInternal(Flow flow, string channel)
        {
            if (String.IsNullOrEmpty(channel))
            {
                // invalid channel name
                return;
            }

            List<Flow> subscribers;
            if (!subscriptions.TryGetValue(channel, out subscribers))
            {
                return;
            }
            int index = subscribers.IndexOf(flow);
            if (index < 0)
            {
                return;
            }
            if (flow.RemoveChannelRef() == 0)
            {
                subscribers.RemoveAt(index);
                if (subscribers.Count == 0)
                {
                    subscriptions.Remove(channel);
                }
            }
        }

        /// <summary>
        /// Represents the set of attached flows for convenient cleanup.
        /// </summary>
        public sealed class Flows : IDisposable
        {
            ~Flows()
            {
                Shutdown();
            }

            /// <summary>
            /// Implements the IDisposable interface.
            /// </summary>
            public void Dispose()
            {
                Shutdown();
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Starts all the attached flows.
            /// </summary>
            public Flows Startup()
            {
                Hub.Startup();
                return this;
            }

            /// <summary>
            /// Stops all the attached flows.
            /// </summary>
            public void Shutdown()
            {
                Hub.Shutdown();
            }
        }
    }
}
