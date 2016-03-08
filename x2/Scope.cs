// Copyright (c) 2013-2016 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace x2
{
    /// <summary>
    /// Request handler method helper class.
    /// </summary>
    public class Scope : IDisposable
    {
        private Event e;
        private Binder.Token? binderToken;

        public Event Event { get { return e; } }

        /// <summary>
        /// Initializes a new Scope object associated with the specified event.
        /// </summary>
        public Scope(Event e)
        {
            this.e = e;
        }

        /// <summary>
        /// Implements IDisposable interface only to support guarded posting.
        /// </summary>
        public void Dispose()
        {
            if (binderToken.HasValue &&
                !Object.ReferenceEquals(binderToken.Value.Key, null))
            {
                Flow.Bind(binderToken.Value);
            }
            Hub.Post(e);
        }

        /// <summary>
        /// Saves the specified binder token for on-exit rebinding.
        /// </summary>
        public void RebindOnExit(Binder.Token binderToken)
        {
            this.binderToken = binderToken;
        }

        /// <summary>
        /// Clears the reserved rebinding.
        /// </summary>
        public void CancelRebinding()
        {
            binderToken = null;
        }
    }
}
