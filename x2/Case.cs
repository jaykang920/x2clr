// Copyright (c) 2013-2016 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2
{
    /// <summary>
    /// Defines methods to initialize/finalize a case. 
    /// </summary>
    public interface ICase
    {
        /// <summary>
        /// Initializes this case with the specified holding flow.
        /// </summary>
        void Setup(Flow holder);

        /// <summary>
        /// Cleans up this case with the specified holding flow.
        /// </summary>
        void Teardown(Flow holder);
    }

    /// <summary>
    /// Represents a finite set of application logic.
    /// </summary>
    public abstract class Case : EventSink, ICase
    {
        /// <summary>
        /// Initializes this case with the specified holding flow.
        /// </summary>
        public void Setup(Flow holder)
        {
            Flow = holder;

            Flow backup = Flow.CurrentFlow;
            Flow.CurrentFlow = holder;

            Setup();

            Flow.CurrentFlow = backup;
        }

        /// <summary>
        /// Cleans up this case with the specified holding flow.
        /// </summary>
        public void Teardown(Flow holder)
        {
            Flow backup = Flow.CurrentFlow;
            Flow.CurrentFlow = holder;

            Teardown();

            Flow.CurrentFlow = backup;

            Dispose();
        }

        /// <summary>
        /// Initializes this case on startup.
        /// </summary>
        protected virtual void Setup() { }

        /// <summary>
        /// Cleans up this case on shutdown.
        /// </summary>
        protected virtual void Teardown() { }
    }

    public class CaseStack : ICase
    {
        private List<ICase> cases;
        private bool activated;

        public CaseStack()
        {
            cases = new List<ICase>();
        }

        public void Add(ICase c)
        {
            lock (cases)
            {
                if (!cases.Contains(c))
                {
                    cases.Add(c);
                }
            }
        }

        public void Remove(ICase c)
        {
            lock (cases)
            {
                cases.Remove(c);
            }
        }

        public void Setup(Flow holder)
        {
            List<ICase> snapshot;
            lock (cases)
            {
                if (activated) { return; }
                activated = true;
                snapshot = new List<ICase>(cases);
            }
            for (int i = 0, count = snapshot.Count; i < count; ++i)
            {
                snapshot[i].Setup(holder);
            }
        }

        public void Teardown(Flow holder)
        {
            List<ICase> snapshot;
            lock (cases)
            {
                if (!activated) { return; }
                activated = false;
                snapshot = new List<ICase>(cases);
            }
            for (int i = snapshot.Count - 1; i >= 0; --i)
            {
                try
                {
                    snapshot[i].Teardown(holder);
                }
                catch (Exception e)
                {
                    Log.Error("{0} {1} Teardown: {2}",
                        holder.Name, snapshot[i].GetType().Name, e);
                }
            }
        }
    }
}
