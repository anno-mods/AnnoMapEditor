using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace AnnoMapEditor.Utilities
{
    internal class Log
    {
        public static string? LogFilePath 
        { 
            get
            {
                if (_logFilePath is null)
                {
                    _logFilePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    if (_logFilePath is not null)
                        _logFilePath = Path.Combine(_logFilePath, "log.txt");
                }
                return _logFilePath;
            }
        }
        private static string? _logFilePath;
        private static bool firstStart = true;

        private static object _lock = new object();

        public static void PrintLine(string message)
        {
            if (LogFilePath is null)
                return;

            if (firstStart)
            {
                // clear file
                using var stream = File.CreateText(LogFilePath);
                firstStart = false;
            }

            lock (_lock)
            {
                File.AppendAllText(LogFilePath, message + "\n");
            }
            Trace.WriteLine(message);
        }
    }
}
