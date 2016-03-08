// Copyright (c) 2013-2016 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace x2
{
    /// <summary>
    /// Helps request handler method with response posting and temporary handler.
    /// </summary>
    public class Scope : IDisposable
    {
        private Event response;
        private Binder.Token? binderToken;

        public Event Response { get { return response; } }

        /// <summary>
        /// Initializes a new Scope object associated with the specified
        /// response event.
        /// </summary>
        public Scope(Event response)
        {
            this.response = response;
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
            Hub.Post(response);
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
