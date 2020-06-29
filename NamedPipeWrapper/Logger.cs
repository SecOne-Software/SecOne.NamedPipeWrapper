using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SecOne.NamedPipeWrapper
{
    public static class Logger
    {
        private static string _filepath;
        private static readonly object _signal = new object();

        public static bool Enabled { get; set; }
        public static string Name { get; set; }
        public static string Path { get; set; }

        static Logger()
        {
            Name = "NamedPipeWrapper";
            Path = $"{Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.System)).FullName}\\Logs\\SecOne";

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                try
                {
                    Write("An unhandled exception occurred.");
                    Write(e.ExceptionObject.ToString());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            };
        }

        public static void Write(string line = null, [CallerMemberName] string callerMemberName = "")
        {
            if (!Enabled) return;

            try
            {
                var method = new StackTrace().GetFrame(1).GetMethod();
                var caller = $"{method.DeclaringType?.Name}.{method.Name}";

                //Detect Async continuation
                if (method.Name.EndsWith("MoveNext")) caller = $"ManagedThread<{Thread.CurrentThread.ManagedThreadId}>.{callerMemberName}";

                var log = $"{DateTimeOffset.UtcNow:u} [{caller}]";

                if (!string.IsNullOrWhiteSpace(line))
                {
                    log += " " + line;
                }

                lock (_signal)
                {
                    var filePath = GetFilePath();

                    Console.WriteLine(log);
                    File.AppendAllText(filePath, log + Environment.NewLine);
                }
            }
            catch
            {

            }
        }

        private static string GetFilePath()
        {
            if (_filepath == null)
            {
                if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);

                _filepath = $"{Path}\\{Name}-{DateTime.Now.Ticks}.txt";
            }

            return _filepath;
        }
    }
}
