// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;

namespace x2
{
    /// <remarks>
    /// All the static and non-static members of this type are not thread-safe.
    /// </remarks>
    public class Process : CaseHolder
    {
        private static Process instance = null;

        /// <exception cref="System.ApplictionException">
        /// Thrown when there is already an instantiated object of x2.Process or 
        /// its subclass.
        /// </exception>
        public Process()
        {
            if (instance != null)
            {
                throw new ApplicationException();
            }
            instance = this;

            // Creates the default(anonymous) hub.
            Hub.Create();
        }

        public static Process GetInstance()
        {
            return instance;
        }

        public static void ReleaseInstance()
        {
            instance = null;
        }

        public void StartUp()
        {
            SetUp();
            caseStack.SetUp();

            // Starts up all the flows attached to the hubs in the current process.
            Hub.StartAllFlows();
        }

        public void ShutDown()
        {
            // Stops all the flows attached to the hubs in the current process.
            Hub.StopAllFlows();

            caseStack.TearDown();
            TearDown();
        }

        protected virtual void SetUp()
        {
        }

        protected virtual void TearDown()
        {
        }
    }
}
