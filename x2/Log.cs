// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Diagnostics;
using System.IO;

namespace x2
{
    public enum LogLevel
    {
        All,
        Trace,
        Debug,
        Info,
        Warning,
        Error,
        None,
    }

    public delegate void LogHandler(LogLevel level, object message);

    public static class Log
    {
        public static LogLevel Level { get; set; }
        public static LogHandler Handler { get; set; }

        public static void Emit(LogLevel level, object message)
        {
            if (Handler == null || Level > level)
            {
                return;
            }
            Handler(level, message);
        }

        public static void Emit(LogLevel level, string format, params object[] args)
        {
            if ((Handler == null) || Level > level)
            {
                return;
            }
            Handler(level, String.Format(format, args));
        }

        public static void Trace(object message)
        {
            Emit(LogLevel.Trace, message);
        }

        public static void Trace(string format, params object[] args)
        {
            Emit(LogLevel.Trace, format, args);
        }

        public static void Debug(object message)
        {
            Emit(LogLevel.Debug, message);
        }

        public static void Debug(string format, params object[] args)
        {
            Emit(LogLevel.Debug, format, args);
        }

        public static void Info(object message)
        {
            Emit(LogLevel.Info, message);
        }

        public static void Info(string format, params object[] args)
        {
            Emit(LogLevel.Info, format, args);
        }

        public static void Warn(object message)
        {
            Emit(LogLevel.Warning, message);
        }

        public static void Warn(string format, params object[] args)
        {
            Emit(LogLevel.Warning, format, args);
        }

        public static void Error(object message)
        {
            Emit(LogLevel.Error, message);
        }

        public static void Error(string format, params object[] args)
        {
            Emit(LogLevel.Error, format, args);
        }
    }
}
