using System;
using System.IO;

namespace TestTask
{
    internal class LogService
    {
        private readonly string _logFilePath;

        public LogService(string logFilePath)
        {
            _logFilePath = logFilePath;

            // Ensure the directory for the log file exists
            var directory = Path.GetDirectoryName(_logFilePath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Ensure the log file exists; create it if it does not
            if (!File.Exists(_logFilePath))
            {
                try
                {
                    // Create the log file if it does not exist
                    using (File.Create(_logFilePath)) { }
                }
                catch (Exception ex)
                {
                    // Log any errors that occur during log file creation
                    Console.WriteLine($"Failed to create log file: {ex.Message}");
                }
            }
        }

        // Method to log a message to both console and log file
        public void Log(string message)
        {
            var logMessage = $"{DateTime.Now}: {message}";

            try
            {
                // Write the log message to the console
                Console.WriteLine(logMessage);

                // Append the log message to the log file
                File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Log any errors that occur during logging
                Console.WriteLine($"Failed to log message: {ex.Message}");
            }
        }
    }
}
