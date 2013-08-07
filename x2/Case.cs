// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2
{
    public interface ICase
    {
        void SetUp();
        void TearDown();
    }

    public class CaseStack : ICase
    {
        private readonly IList<ICase> cases;
        private bool activated;

        public CaseStack()
        {
            cases = new List<ICase>();
            activated = false;
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
            c.SetUp();
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
            c.TearDown();
        }

        public void SetUp()
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
                c.SetUp();
            }
        }

        public void TearDown()
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
                c.TearDown();
            }
        }
    }
}
