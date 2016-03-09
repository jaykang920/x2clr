// Copyright (c) 2013-2016 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace x2
{
    /// <summary>
    /// Helps code block scope based cleanup.
    /// </summary>
    public class Scope : IDisposable
    {
        private Binder.Token? binderToken;
        private Event e;

        /// <summary>
        /// Gets or sets the binder token to be recovered on disposal.
        /// </summary>
        public Binder.Token? BinderToken
        {
            get { return binderToken; }
            set { binderToken = value; }
        }

        /// <summary>
        /// Gets the event to be posted on disposal.
        /// </summary>
        public Event Event { get { return e; } }

        /// <summary>
        /// Initializes a new Scope object.
        /// </summary>
        public Scope()
        {
        }

        /// <summary>
        /// Initializes a new Scope object with the specified event.
        /// </summary>
        public Scope(Event e)
        {
            this.e = e;
        }

        /// <summary>
        /// Implements IDisposable interface.
        /// </summary>
        public void Dispose()
        {
            if (binderToken.HasValue &&
                !Object.ReferenceEquals(binderToken.Value.Key, null))
            {
                Flow.Bind(binderToken.Value);
            }
            if (!Object.ReferenceEquals(e, null))
            {
                Hub.Post(e);
            }
        }
    }
}
