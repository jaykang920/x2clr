// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Diagnostics;
using System.IO;

namespace x2
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    public delegate void LogHandler(LogLevel level, object message);

    public static class Log
    {
        public static LogLevel Level { get; set; }
        public static LogHandler Handler { get; set; }

        #region Debug

        public static void Debug(object message)
        {
            if ((Handler == null) || Level > LogLevel.Debug)
            {
                return;
            }
            Handler(LogLevel.Debug, message);
        }

        public static void Debug(string format, params object[] args)
        {
            if ((Handler == null) || Level > LogLevel.Debug)
            {
                return;
            }
            Handler(LogLevel.Debug, String.Format(format, args));
        }

        #endregion

        #region Info

        public static void Info(object message)
        {
            if ((Handler == null) || Level > LogLevel.Info)
            {
                return;
            }
            Handler(LogLevel.Info, message);
        }

        public static void Info(string format, params object[] args)
        {
            if ((Handler == null) || Level > LogLevel.Info)
            {
                return;
            }
            Handler(LogLevel.Info, String.Format(format, args));
        }

        #endregion

        #region Warning

        public static void Warn(object message)
        {
            if ((Handler == null) || Level > LogLevel.Warning)
            {
                return;
            }
            Handler(LogLevel.Warning, message);
        }

        public static void Warn(string format, params object[] args)
        {
            if ((Handler == null) || Level > LogLevel.Warning)
            {
                return;
            }
            Handler(LogLevel.Warning, String.Format(format, args));
        }

        #endregion

        #region Error

        public static void Error(object message)
        {
            if ((Handler == null) || Level > LogLevel.Error)
            {
                return;
            }
            Handler(LogLevel.Error, message);
        }

        public static void Error(string format, params object[] args)
        {
            if ((Handler == null) || Level > LogLevel.Error)
            {
                return;
            }
            Handler(LogLevel.Error, String.Format(format, args));
        }

        #endregion
    }
}
