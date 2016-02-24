// Copyright (c) 2013-2016 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Configuration;

namespace x2
{
    /// <summary>
    /// Provides the global configuration properties.
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// Gets or sets the minimum log level.
        /// </summary>
        public static LogLevel LogLevel { get; set;}

        public static class Buffer
        {
            public static class SizeExponent
            {
                /// <summary>
                /// Gets or sets the buffer chunk size exponent n in 2^n.
                /// </summary>
                public static int Chunk { get; set; }
                /// <summary>
                /// Gets or sets the buffer segment size exponent n in 2^n.
                /// </summary>
                public static int Segment { get; set; }
            }

            /// <summary>
            /// Gets the buffer chunk size in bytes.
            /// </summary>
            public static int ChunkSize
            {
                get { return (1 << SizeExponent.Chunk); }
            }
            /// <summary>
            /// Gets the buffer segment size in bytes.
            /// </summary>
            public static int SegmentSize
            {
                get { return (1 << SizeExponent.Segment); }
            }

            public static class RoomFactor
            {
                public static int MinLevel { get; set; }
                public static int MaxLevel { get; set; }
            }
        }

        public static class Coroutine
        {
            /// <summary>
            /// Gets or sets the maximum number of wait handles.
            /// </summary>
            public static int MaxWaitHandles { get; set; }
            /// <summary>
            /// Gets or sets the default wait timeout in seconds.
            /// </summary>
            public static double DefaultTimeout { get; set; }
        }

        public static class Flow
        {
            public static class Logging
            {
                public static class SlowHandler
                {
                    public static LogLevel LogLevel { get; set; }
                    public static int Threshold { get; set; }
                }

                public static class LongQueue
                {
                    public static LogLevel LogLevel { get; set; }
                    public static int Threshold { get; set; }
                }
            }
        }

        public static int HeartbeatInterval { get; set; }

        public static int MaxLinkHandles { get; set; }

        static Config()
        {
            // Default values

            LogLevel = LogLevel.Info;

            // SizeExponent.Chunk >= SizeExponent.Segment
            Buffer.SizeExponent.Chunk = 24;  // 16MB
            Buffer.SizeExponent.Segment = 12;  // 4KB
            Buffer.RoomFactor.MinLevel = 0;  // x1
            Buffer.RoomFactor.MaxLevel = 3;  // x8

            Coroutine.MaxWaitHandles = 1024;
            Coroutine.DefaultTimeout = 30.0;  // in seconds

            Flow.Logging.SlowHandler.LogLevel = LogLevel.Warning;
            Flow.Logging.SlowHandler.Threshold = 100;  // in milliseconds
            Flow.Logging.LongQueue.LogLevel = LogLevel.Error;
            Flow.Logging.LongQueue.Threshold = 1000;

            HeartbeatInterval = 5;  // in seconds

            MaxLinkHandles = 65536;
        }

        /// <summary>
        /// Loads the configuration properties from the application
        /// configuration.
        /// </summary>
        public static void Load()
        {
            ConfigSection section = (ConfigSection)
                ConfigurationManager.GetSection("x2clr");

            LogLevel = section.Log.Level;

            BufferElement buffer = section.Buffer;
            Buffer.SizeExponent.Chunk = buffer.SizeExponent.Chunk;
            Buffer.SizeExponent.Segment = buffer.SizeExponent.Segment;
            Buffer.RoomFactor.MinLevel = buffer.RoomFactor.MinLevel;
            Buffer.RoomFactor.MaxLevel = buffer.RoomFactor.MaxLevel;

            Coroutine.MaxWaitHandles = section.Coroutine.MaxWaitHandles;
            Coroutine.DefaultTimeout = section.Coroutine.DefaultTimeout;

            FlowLoggingElement logging = section.Flow.Logging;
            Flow.Logging.SlowHandler.LogLevel = logging.SlowHandler.LogLevel;
            Flow.Logging.SlowHandler.Threshold = logging.SlowHandler.Threshold;
            Flow.Logging.LongQueue.LogLevel = logging.LongQueue.LogLevel;
            Flow.Logging.LongQueue.Threshold = logging.LongQueue.Threshold;

            HeartbeatInterval = section.Heartbeat.Interval;

            MaxLinkHandles = section.Link.MaxHandles;
        }
    }
}
