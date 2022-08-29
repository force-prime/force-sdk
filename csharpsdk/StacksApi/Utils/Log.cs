using System;
using System.Diagnostics;

namespace StacksForce.Utils
{
    static public class Log
    {
        public enum Severity { Debug = 0, Info = 1, Warning = 2, Fatal = 4 };

        static public event Action<string, Severity> LogMethod = (m, s) => {
#if DEBUG
            if (s >= Severity.Warning)
                Console.WriteLine($"{s}: {m}");
#endif
        };

        static public Action<string, Severity> NoListenersHandler { get; set; } = DefaultNoListenersHandler;

        static public int LogLevel = 0;

        [Conditional("DEBUG")]
        static public void Trace(string msg)
        {
            DoLog(msg, Severity.Debug);
        }

        static public void Debug(string msg)
        {
            DoLog(msg, Severity.Debug);
        }

        static public void Info(string msg)
        {
            DoLog(msg, Severity.Info);
        }

        static public void Warning(string msg)
        {
            DoLog(msg, Severity.Warning);
        }

        static public void Fatal(string msg)
        {
            DoLog(msg, Severity.Fatal);
        }

        static public void Add(string msg, Severity severity)
        {
            if ((int)severity < LogLevel)
                return;

            DoLog(msg, severity);
        }

        private static void DoLog(string msg, Severity severity) {
            if (LogMethod != null)
                LogMethod.Invoke(msg, severity);
            else
                NoListenersHandler?.Invoke(msg, severity);
        }

        private static void DefaultNoListenersHandler(string msg, Severity severity)
        {
#if DEBUG
            Console.WriteLine($"Silent log message: {severity} {msg}");
#endif
        }
    }
}
