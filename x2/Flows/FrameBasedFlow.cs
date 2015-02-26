// Copyright (c) 2013-2015 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public const long TicksInMillisecond = 10000;  // 100-nanosecond tick
        public const long TicksInSecond = 1000 * TicksInMillisecond;

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
        public double DeltaTime
        {
            get { return new TimeSpan(deltaTicks).TotalSeconds; }
        }

        /// <summary>
        /// Number of ticks representing the start UTC DateTime of the current frame.
        /// </summary>
        public long CurrentTicks { get { return currentTicks; } }

        /// <summary>
        /// Gets the start local DateTime of the current frame.
        /// </summary>
        public DateTime Now { get { return new DateTime(currentTicks).ToLocalTime(); } }

        /// <summary>
        /// Gets the start UTC DateTime of the current frame.
        /// </summary>
        public DateTime UtcNow { get { return new DateTime(currentTicks); } }

        public void Initialize()
        {
            startTicks = DateTime.UtcNow.Ticks;
            lastTicks = startTicks;
            currentTicks = lastTicks;
        }

        public void BeforeUpdate()
        {
            currentTicks = DateTime.UtcNow.Ticks;
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

        private volatile bool shouldStop;

        public long Resolution { get; set; }

        public Time Time { get; private set; }

        protected FrameBasedFlow()
            : this(new UnboundedQueue<Event>())
        {
        }
        
        protected FrameBasedFlow(IQueue<Event> queue)
        {
            this.queue = queue;
            syncRoot = new Object();
            thread = null;

            Resolution = Time.TicksInMillisecond;  // default 1ms resolution

            Time = new Time();
        }

        public override void Feed(Event e)
        {
            if (queue != null)
            {
                queue.Enqueue(e);
            }
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
                thread.Name = name;
                thread.Start();
                if (queue != null)
                {
                    queue.Enqueue(new FlowStart());
                }
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
                if (queue != null)
                {
                    queue.Close(new FlowStop());
                }
                else
                {
                    shouldStop = true;
                }
                thread.Join();
                thread = null;

                caseStack.TearDown(this);
                TearDown();
            }
        }

        private void Run()
        {
            currentFlow = this;
            if (queue != null)
            {
                handlerChain = new List<Handler>();
            }
            stopwatch = new Stopwatch();

            StartInternal();

            while (!shouldStop)
            {
                UpdateInternal();

                if (queue != null)
                {
                    while ((DateTime.UtcNow.Ticks - Time.CurrentTicks) < Resolution)
                    {
                        Event e;
                        if (queue.TryDequeue(out e))
                        {
                            Dispatch(e);

                            if (e.GetTypeId() == (int)BuiltinType.FlowStop)
                            {
                                shouldStop = true;
                                break;
                            }
                        }
                        else
                        {
                            Thread.Sleep(1);
                            break;
                        }
                    }
                }
                else
                {
                    var tickDelta = DateTime.UtcNow.Ticks - Time.CurrentTicks;
                    var delay = (tickDelta < Resolution ?
                        (int)((Resolution - tickDelta) / Time.TicksInMillisecond) : 0);
                    Thread.Sleep(delay);
                }
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
