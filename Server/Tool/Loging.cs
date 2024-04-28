using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Logging
{
    public class Debug
    {
        public static string prefix = "";
        static StringBuilder _traceSb = new StringBuilder();
        public static string TraceSavePath;
        public static int traceDumpLen = 128 * 1024;
        private static Stream _stream;
        ~Debug()
        {
            Debug.FlushTrace();
        }
        //[Conditional("DEBUG")]
        //[Conditional("LOG_TRACE")]
        public static void Trace(string msg, bool isNeedLogTrace = false, bool isNewLine = true)
        {
            if (isNewLine)
            { 
                _traceSb.AppendLine(msg);
            }
            else
            {
                _traceSb.Append(msg);
            }

            if (isNeedLogTrace)
            {
                StackTrace st = new StackTrace(true);
                StackFrame[] sf = st.GetFrames();
                for (int i = 0; i < sf.Length; ++i)
                {
                    var frame = sf[i];
                    _traceSb.AppendLine($"at {frame.GetMethod().DeclaringType.FullName}::frame.GetMethod().Name  in  {frame.GetFileName()} cs:line  { frame.GetFileLineNumber()}" );
                }
            }

            if (_traceSb.Length > traceDumpLen)
            {
                FlushTrace();
            }
        }

        public static void FlushTrace()
        {
            if (string.IsNullOrEmpty(TraceSavePath))
                return;
            if (_stream == null)
            {
                var dir = Path.GetDirectoryName(TraceSavePath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                _stream = File.Open(TraceSavePath, FileMode.OpenOrCreate, FileAccess.Write);
            }

            var bytes = UTF8Encoding.Default.GetBytes(_traceSb.ToString());
            _stream.Write(bytes, 0, bytes.Length);
            _stream.Flush();
            _traceSb.Clear();
        }

        public static void Log(object obj, params object[] args)
        {
            Logger.Info(0, prefix + obj.ToString(), args);
        }
        public static void Log(string format, params object[] args)
        {
            Logger.Info(0, prefix + format, args);
        }

        public static void LogFormat(string format, params object[] args)
        {
            Logger.Info(0, prefix + format, args);
        }
        public static void LogWarning(string format, params object[] args)
        {
            Logger.Warn(0, prefix + format, args);
        }
        public static void LogError(string format, params object[] args)
        {
            Logger.Err(0, prefix + format, args);
        }

        public static void LogError(Exception e)
        {
            Logger.Err(0, prefix + e.ToString());
        }

        public static void LogErrorFormat(string format, params object[] args)
        {
            Logger.Err(0, prefix + format, args);
        }

        [Conditional("DEBUG")]
        public static void Assert(bool val, string msg = "")
        {
            Logger.Assert(0, val, prefix + msg);
        }
    }
    public class DebugInstance
    {
        private string _prefix = "";

        public DebugInstance(string prefix)
        {
            this._prefix = prefix;
        }

        public void SetPrefix(string prefix)
        {
            _prefix = prefix;
        }

        public void Log(string format, params object[] args)
        {
            Logger.Info(0, _prefix + format, args);
        }

        public void LogFormat(string format, params object[] args)
        {
            Logger.Info(0, _prefix + format, args);
        }

        public void LogError(string format, params object[] args)
        {
            Logger.Err(0, _prefix + format, args);
        }

        public void LogError(Exception e)
        {
            Logger.Err(0, _prefix + e.ToString());
        }

        public void LogErrorFormat(string format, params object[] args)
        {
            Logger.Err(0, _prefix + format, args);
        }

        public void Assert(bool val, string msg = "")
        {
            Logger.Assert(0, val, _prefix + msg);
        }
    }
    public class BaseLogger
    {
        protected DebugInstance Debug;

        public void SetLogger(DebugInstance logger)
        {
            this.Debug = logger;
        }

        protected void Log(string format, params object[] args)
        {
            Debug?.Log(format, args);
        }

        protected void LogFormat(string format, params object[] args)
        {
            Debug?.LogFormat(format, args);
        }

        protected void LogError(string format, params object[] args)
        {
            Debug?.LogError(format, args);
        }

        protected void LogError(Exception e)
        {
            Debug?.LogError(e);
        }

        protected void LogErrorFormat(string format, params object[] args)
        {
            Debug?.LogErrorFormat(format, args);
        }

        protected void Assert(bool val, string msg = "")
        {
            Debug?.Assert(val, msg);
        }
    }

    [Flags]
    public enum LogSeverity
    {
        Exception = 1,
        Error = 2,
        Warn = 4,
        Info = 8,
        Trace = 16
    }
    public class LogEventArgs : EventArgs
    {
        public LogSeverity LogSeverity { get; }

        public string Message { get; }

        public LogEventArgs(LogSeverity logSeverity, string message)
        {
            LogSeverity = logSeverity;
            Message = message;
        }
    }
    public static class Logger
    {
        public static LogSeverity LogSeverityLevel =
            LogSeverity.Info | LogSeverity.Warn | LogSeverity.Error | LogSeverity.Exception;

        public static event EventHandler<LogEventArgs> OnMessage = DefaultServerLogHandler;
        public static Action<bool, string> OnAssert;

        public static void SetLogAllSeverities()
        {
            LogSeverityLevel = LogSeverity.Trace | LogSeverity.Info | LogSeverity.Warn | LogSeverity.Error |
                               LogSeverity.Exception;
        }

        public static void Err(object sender, string message, params object[] args)
        {
            LogMessage(sender, LogSeverity.Error, message, args);
        }

        public static void Warn(object sender, string message, params object[] args)
        {
            LogMessage(sender, LogSeverity.Warn, message, args);
        }

        public static void Info(object sender, string message, params object[] args)
        {
            LogMessage(sender, LogSeverity.Info, message, args);
        }

        public static void Trace(object sender, string message, params object[] args)
        {
            LogMessage(sender, LogSeverity.Trace, message, args);
        }

        public static void Assert(object sender, bool val, string message)
        {
            if (!val)
            {
                LogMessage(sender, LogSeverity.Error, "AssertFailed!!! " + message);
            }
        }

        private static void LogMessage(object sender, LogSeverity sev, string format, params object[] args)
        {
            //Console.WriteLine(format);
            if (OnMessage != null && (LogSeverityLevel & sev) != 0)
            {
                var message = (args != null && args.Length > 0) ? string.Format(format, args) : format;
                OnMessage.Invoke(sender, new LogEventArgs(sev, message));
            }
        }

        static StringBuilder _logBuffer = new StringBuilder();
        public static void DefaultServerLogHandler(object sernder, LogEventArgs logArgs)
        {
            if ((LogSeverity.Error & logArgs.LogSeverity) != 0
                || (LogSeverity.Exception & logArgs.LogSeverity) != 0
            )
            {
                StackTrace st = new StackTrace(true);
                StackFrame[] sf = st.GetFrames();
                for (int i = 4; i < sf.Length; ++i)
                {
                    var frame = sf[i];
                    _logBuffer.AppendLine(frame.GetMethod().DeclaringType.FullName + "::" + frame.GetMethod().Name +
                                  " Line=" + frame.GetFileLineNumber());
                }
            }

            Console.WriteLine(logArgs.Message);
            if (_logBuffer.Length != 0)
            {
                Console.WriteLine(_logBuffer.ToString());
                _logBuffer.Length = 0;
                _logBuffer.Clear();
            }
        }
    }
}
