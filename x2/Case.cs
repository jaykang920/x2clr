// Copyright (c) 2013-2015 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2
{
    public interface ICase
    {
        void SetUp(Flow holder);
        void TearDown(Flow holder);
    }

    public abstract class Case : EventSink, ICase
    {
        public void SetUp(Flow holder)
        {
            Flow = holder;

            Flow backup = Flow.CurrentFlow;
            Flow.CurrentFlow = holder;

            SetUp();

            Flow.CurrentFlow = backup;
        }

        public void TearDown(Flow holder)
        {
            Flow backup = Flow.CurrentFlow;
            Flow.CurrentFlow = holder;

            TearDown();

            Flow.CurrentFlow = backup;

            Dispose();
        }

        /// <summary>
        /// Initializes this case on startup.
        /// </summary>
        protected virtual void SetUp() { }
        /// <summary>
        /// Cleans up this case on shutdown.
        /// </summary>
        protected virtual void TearDown() { }
    }

    public class CaseStack : ICase
    {
        private readonly IList<ICase> cases;
        private bool activated;

        public CaseStack()
        {
            cases = new List<ICase>();
        }

        public void Add(ICase c)
        {
            lock (cases)
            {
                if (cases.Contains(c))
                {
                    return;
                }
                cases.Add(c);
                if (!activated)
                {
                    return;
                }
            }
            //c.SetUp();
        }

        public void Remove(ICase c)
        {
            lock (cases)
            {
                if (!cases.Remove(c) || !activated)
                {
                    return;
                }
            }
            //c.TearDown();
        }

        public void SetUp(Flow holder)
        {
            List<ICase> snapshot;
            lock (cases)
            {
                if (activated)
                {
                    return;
                }
                activated = true;
                snapshot = new List<ICase>(cases);
            }
            for (int i = 0, count = snapshot.Count; i < count; ++i)
            {
                snapshot[i].SetUp(holder);
            }
        }

        public void TearDown(Flow holder)
        {
            List<ICase> snapshot;
            lock (cases)
            {
                if (!activated)
                {
                    return;
                }
                activated = false;
                snapshot = new List<ICase>(cases);
            }
            for (int i = snapshot.Count - 1; i >= 0; --i)
            {
                snapshot[i].TearDown(holder);
            }
        }
    }
}
