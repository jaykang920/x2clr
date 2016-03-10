﻿// Copyright (c) 2013-2016 Jae-jun Kang
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
        private DateTime startTime;
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
        /// A delegate type for hooking up Dispose notifications.
        /// </summary>
        public delegate void CleanupHandler(Event e);

        /// <summary>
        /// An event that clients can bind custom actions to be executed when
        /// this Scope object is disposed.
        /// </summary>
        public event CleanupHandler Cleanup;

        /// <summary>
        /// Initializes a new Scope object.
        /// </summary>
        public Scope()
        {
            startTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Initializes a new Scope object with the specified event.
        /// </summary>
        public Scope(Event e)
        {
            startTime = DateTime.UtcNow;
            this.e = e;
        }

        /// <summary>
        /// Implements IDisposable interface.
        /// </summary>
        public void Dispose()
        {
            if (Cleanup != null)
            {
                Cleanup(e);
            }

            if (binderToken.HasValue &&
                !Object.ReferenceEquals(binderToken.Value.Key, null))
            {
                Flow.Bind(binderToken.Value);
            }

            if (!Object.ReferenceEquals(e, null))
            {
                Hub.Post(e);
            }

            DateTime endTime = DateTime.UtcNow;
            long totalMillis = (long)(endTime - startTime).TotalMilliseconds;
            if (totalMillis >= Config.Flow.Logging.SlowScope.Threshold)
            {
                Log.Emit(Config.Flow.Logging.SlowScope.LogLevel,
                    "{0} slow scope {1:#,0}ms on {2}",
                    Flow.CurrentFlow.Name, totalMillis, e);
            }
        }
    }
}
