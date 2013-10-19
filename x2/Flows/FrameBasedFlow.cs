// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Threading;

using x2.Events;
using x2.Queues;

namespace x2.Flows
{
    /// <summary>
    /// Utility class to handle time information within a frame-based flow.
    /// </summary>
    public class Time
    {
        private long startTicks;
        private long lastTicks;
        private long currentTicks;
        private long deltaTicks;

        /// <summary>
        /// Gets the number of ticks it took to complete the last frame.
        /// </summary>
        public long DeltaTicks { get { return deltaTicks; } }

        /// <summary>
        /// Gets the time in seconds it took to complete the last frame.
        /// </summary>
        public float DeltaTime
        {
            get { return (float)(new TimeSpan(deltaTicks).TotalSeconds); }
        }

        /// <summary>
        /// Gets the start DateTime of the current frame.
        /// </summary>
        public DateTime Now { get { return new DateTime(currentTicks); } }

        public void Initialize()
        {
            startTicks = DateTime.Now.Ticks;
            lastTicks = startTicks;
            currentTicks = lastTicks;
        }

        public void BeforeUpdate()
        {
            currentTicks = DateTime.Now.Ticks;
            deltaTicks = currentTicks - lastTicks;
        }

        public void AfterUpdate()
        {
            lastTicks = currentTicks;
        }
    }

    /// <summary>
    /// Abstract base class for frame-based (looping) execution flows.
    /// </summary>
    public abstract class FrameBasedFlow : Flow
    {
        protected readonly IQueue<Event> queue;
        protected readonly object syncRoot;
        protected Thread thread;

        public Time Time { get; private set; }

        protected long previousTicks;

        protected FrameBasedFlow()
            : this(new UnboundedQueue<Event>())
        {
            Time = new Time();
        }
        
        protected FrameBasedFlow(IQueue<Event> queue)
            : base(new Binder())
        {
            this.queue = queue;
            syncRoot = new Object();
            thread = null;
        }

        public override void Feed(Event e)
        {
            queue.Enqueue(e);
        }

        public override void StartUp()
        {
            lock (syncRoot)
            {
                if (thread != null)
                {
                    return;
                }

                SetUp();
                caseStack.SetUp(this);
                thread = new Thread(this.Run);
                thread.Start();
                queue.Enqueue(new FlowStart());
            }
        }

        public override void ShutDown()
        {
            lock (syncRoot)
            {
                if (thread == null)
                {
                    return;
                }
                queue.Close(new FlowStop());
                thread.Join();
                thread = null;

                caseStack.TearDown(this);
                TearDown();
            }
        }

        private void Run()
        {
            currentFlow = this;
            handlerChain = new List<IHandler>();

            StartInternal();

            while (true)
            {
                Event e;
                if (queue.TryDequeue(out e))
                {
                    Dispatch(e);

                    if (e.GetTypeId() == (int)BuiltinType.FlowStop)
                    {
                        break;
                    }
                }

                UpdateInternal();

                Thread.Sleep(1);
            }

            Stop();

            handlerChain = null;
            currentFlow = null;
        }

        private void StartInternal()
        {
            Time.Initialize();

            Start();
        }

        private void UpdateInternal()
        {
            Time.BeforeUpdate();

            Update();

            Time.AfterUpdate();
        }

        protected abstract void Start();
        protected abstract void Stop();

        protected abstract void Update();
    }
}
