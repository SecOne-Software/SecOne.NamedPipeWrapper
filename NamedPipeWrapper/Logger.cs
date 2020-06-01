using System;
using System.Diagnostics;
using System.IO;

namespace SecOne.NamedPipeWrapper
{
    public static class Logger
    {
        private static string _filepath;
        private readonly static object _lock = new object();

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

        public static void Write(string line = null, string caller = null)
        {
            if (string.IsNullOrWhiteSpace(caller))
            {
                var method = new StackTrace().GetFrame(1).GetMethod();

                try
                {
                    if (method.DeclaringType.Name != null && method.DeclaringType.Name.StartsWith("<"))
                    {
                        var name = method.DeclaringType.Name;
                        var pos = name.IndexOf('>');
                        caller = name.Substring(0, pos+1);
                    }
                    else
                    {
                        caller = $"{method.DeclaringType?.Name}.{method.Name}";
                    }
                }
                catch
                {
                    caller = $"{method.DeclaringType?.Name}.{method.Name}";
                }
            }

            var log = $"{DateTimeOffset.UtcNow:u} [{caller}]";

            if (!string.IsNullOrWhiteSpace(line))
            {
                log += " " + line;
            }

            if (_filepath == null) {
                var now = DateTime.Now;
                string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                path = path.Replace("file:\\", "");

                path = "C:\\Windows";
                //path = AppDomain.CurrentDomain.BaseDirectory;

                _filepath = $"{path}\\npw-{now.Ticks}.txt";
            }

            lock (_lock)
            {
                Console.WriteLine(log);
                File.AppendAllText(_filepath, log + Environment.NewLine);
            }
        }
    }
}
