// Copyright (c) 2013-2015 Jae-jun Kang
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
        // Log

        /// <summary>
        /// Gets or sets the minimum log level.
        /// </summary>
        public static LogLevel LogLevel { get; set;}

        // Segment buffer (chunk size >= segment size)

        /// <summary>
        /// Gets or sets the buffer chunk size in 2^n.
        /// </summary>
        public static int ChunkSizeExponent { get; set; }
        /// <summary>
        /// Gets or sets the buffer segment size in 2^n.
        /// </summary>
        public static int SegmentSizeExponent { get; set; }
        /// <summary>
        /// Gets the buffer chunk size in bytes.
        /// </summary>
        public static int ChunkSize
        {
            get { return (1 << ChunkSizeExponent); }
        }
        /// <summary>
        /// Gets the buffer segment size in bytes.
        /// </summary>
        public static int SegmentSize
        {
            get { return (1 << SegmentSizeExponent); }
        }

        // Coroutine

        /// <summary>
        /// Gets or sets the maximum number of wait handles.
        /// </summary>
        public static int MaxWaitHandles { get; set; }
        /// <summary>
        /// Gets or sets the default wait timeout in seconds.
        /// </summary>
        public static double DefaultWaitTimeout { get; set; }

        // Flow

        public static LogLevel DefaultSlowHandlerLogLevel { get; set; }
        public static int DefaultSlowHandlerLogThreshold { get; set; }

        static Config()
        {
            // Default values

            LogLevel = LogLevel.Info;

            ChunkSizeExponent = 24;  // 16MB
            SegmentSizeExponent = 12;  // 4KB

            MaxWaitHandles = 1024;
            DefaultWaitTimeout = 30.0;

            DefaultSlowHandlerLogLevel = LogLevel.Warning;
            DefaultSlowHandlerLogThreshold = 100;
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

            ChunkSizeExponent = section.Buffer.ChunkSizeExponent;
            SegmentSizeExponent = section.Buffer.SegmentSizeExponent;

            MaxWaitHandles = section.Coroutine.MaxWaitHandles;
            DefaultWaitTimeout = section.Coroutine.DefaultTimeout;

            DefaultSlowHandlerLogLevel = section.Flow.DefaultSlowHandlerLogLevel;
            DefaultSlowHandlerLogThreshold = section.Flow.DefaultSlowHandlerLogThreshold;
        }
    }
}
