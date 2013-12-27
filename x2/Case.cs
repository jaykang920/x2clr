// Copyright (c) 2013 Jae-jun Kang
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

            SetUp();
        }

        public void TearDown(Flow holder)
        {
            TearDown();

            CleanUp();
        }

        protected virtual void SetUp() { }
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
            IEnumerable<ICase> snapshot;
            lock (cases)
            {
                if (activated)
                {
                    return;
                }
                activated = true;
                snapshot = new List<ICase>(cases);
            }
            foreach (ICase c in snapshot)
            {
                c.SetUp(holder);
            }
        }

        public void TearDown(Flow holder)
        {
            IEnumerable<ICase> snapshot;
            lock (cases)
            {
                if (!activated)
                {
                    return;
                }
                activated = false;
                snapshot = new Stack<ICase>(cases);
            }
            foreach (ICase c in snapshot)
            {
                c.TearDown(holder);
            }
        }
    }
}
