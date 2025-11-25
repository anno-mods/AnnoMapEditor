using System;

namespace AnnoMapEditor.Utilities
{
    public class Logger<T>
    {
        private readonly string _logTemplate;


        public Logger()
        {
            _logTemplate = "[{0}] [" + typeof(T).Name + "] [{1}]: {2}";
        }


        private static string GetTimestamp()
            => DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");

        public void LogInformation(string message)
        {
            Console.WriteLine(string.Format(_logTemplate,
                GetTimestamp(), 
                "Information", 
                message));
        }

        public void LogWarning(string message)
        {
            Console.WriteLine(string.Format(_logTemplate,
                GetTimestamp(),
                "Warning",
                message));
        }

        public void LogError(string message, Exception? ex = null)
        {
            Console.WriteLine(string.Format(_logTemplate,
                GetTimestamp(),
                "Error",
                message));

            if (ex != null)
                Console.WriteLine(ex.Message);
        }
    }
}
