using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SecOne.NamedPipeWrapper
{
    internal static class Logger
    {
        private static string _filepath;
        private static readonly object _signal = new object();

        static Logger()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                try
                {
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
                    if (_filepath == null)
                    {
                        var path = GetPath();

                        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                        _filepath = $"{path}\\{GetName()}-{DateTime.Now.Ticks}.txt";
                    }

                    Console.WriteLine(log);
                    File.AppendAllText(_filepath, log + Environment.NewLine);
                }
            }
            catch
            {

            }
        }

        private static string GetName()
        {
            return "NamedPipeWrapper";
        }

        private static string GetPath()
        {
            var path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.System)).FullName;
            return $"{path}\\Logs\\SecOne";
        }
    }
}
