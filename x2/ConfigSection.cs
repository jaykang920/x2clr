// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Configuration;

namespace x2
{
    /// <summary>
    /// The x2clr configuration section handler.
    /// </summary>
    public sealed class ConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("log")]
        public LogElement Log
        {
            get { return (LogElement)this["log"]; }
            set { this["log"] = value; }
        }

        [ConfigurationProperty("buffer")]
        public BufferElement Buffer
        {
            get { return (BufferElement)this["buffer"]; }
            set { this["buffer"] = value; }
        }

        [ConfigurationProperty("coroutine")]
        public CoroutineElement Coroutine
        {
            get { return (CoroutineElement)this["coroutine"]; }
            set { this["coroutine"] = value; }
        }

        [ConfigurationProperty("flow")]
        public FlowElement Flow
        {
            get { return (FlowElement)this["flow"]; }
            set { this["flow"] = value; }
        }
    }

    /// <summary>
    /// The x2clr/log configuration element handler.
    /// </summary>
    public class LogElement : ConfigurationElement
    {
        [ConfigurationProperty("level")]
        public LogLevel Level
        {
            get { return (LogLevel)this["level"]; }
            set { this["level"] = value; }
        }
    }

    /// <summary>
    /// The x2clr/buffer configuration element handler.
    /// </summary>
    public class BufferElement : ConfigurationElement
    {
        [ConfigurationProperty("chunkSizeExponent")]
        public int ChunkSizeExponent
        {
            get { return (int)this["chunkSizeExponent"]; }
            set { this["chunkSizeExponent"] = value; }
        }

        [ConfigurationProperty("segmentSizeExponent")]
        public int SegmentSizeExponent
        {
            get { return (int)this["segmentSizeExponent"]; }
            set { this["segmentSizeExponent"] = value; }
        }
    }

    /// <summary>
    /// The x2clr/coroutine configuration element handler.
    /// </summary>
    public class CoroutineElement : ConfigurationElement
    {
        [ConfigurationProperty("maxWaitHandles")]
        public int MaxWaitHandles
        {
            get { return (int)this["maxWaitHandles"]; }
            set { this["maxWaitHandles"] = value; }
        }

        [ConfigurationProperty("defaultTimeout")]
        public double DefaultTimeout
        {
            get { return (double)this["defaultTimeout"]; }
            set { this["defaultTimeout"] = value; }
        }
    }

    /// <summary>
    /// The x2clr/flow configuration element handler.
    /// </summary>
    public class FlowElement : ConfigurationElement
    {
        [ConfigurationProperty("defaultSlowHandlerLogLevel")]
        public LogLevel DefaultSlowHandlerLogLevel
        {
            get { return (LogLevel)this["defaultSlowHandlerLogLevel"]; }
            set { this["defaultSlowHandlerLogLevel"] = value; }
        }

        [ConfigurationProperty("defaultSlowHandlerLogThreshold")]
        public int DefaultSlowHandlerLogThreshold
        {
            get { return (int)this["defaultSlowHandlerLogThreshold"]; }
            set { this["defaultSlowHandlerLogThreshold"] = value; }
        }
    }
}
