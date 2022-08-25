using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using SqlServer.Helpers;

namespace SqlServer.Logging
{
    public class DiagnosticsLogWriterFactory : ILogWriterFactory
    {
        public ILogWriter Create(string name)
        {
            try
            {
                var logFolder = ExpressApp.GetEnvVariableSafe("BO_InternalLoggerFolder");
                if (logFolder != null)
                {
                    var path = string.Format("BO_Internal_{0:yyyyMMdd}_{1}.log", DateTime.Now, ExpressApp.ProcessId);
                    path = Path.Combine(logFolder, path);
                    return new DiagnosticsFileLogWriter(name, path);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return new DebugLogWriter(true);
        }

        public sealed class DiagnosticsFileLogWriter : ILogWriter
        {
            private readonly string _name;
            private readonly string _logFile;

            public DiagnosticsFileLogWriter(string name, string logFile)
            {
                _name = name;
                _logFile = logFile;
                var directoryFullName = new DirectoryInfo(_logFile).FullName;
                var directoryInfo = DirectoryHelper.CreateDirectory(directoryFullName);
                var fileInfo = new FileInfo(_logFile);
                _logFile = Path.Combine(directoryInfo.FullName, fileInfo.Name);
                LogInternalDiagnostics();
            }

            public void Write(string format, params object[] args)
            {
                try
                {
                    DateTime eventTime = DateTime.Now;
                    var builder = new StringBuilder();
                    builder.Append(eventTime.ToString("o"));
                    builder.Append('\t').Append(_name);
                    builder.Append('\t').AppendLine(string.Format(format, args));
                    File.AppendAllText(_logFile, builder.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            public void Log(string format, params object[] args)
            {
                Write(string.Format(format, args));
            }

            private void LogInternalDiagnostics()
            {
                try
                {
                    var process = Process.GetCurrentProcess();

                    Log("Started AppLogger for process '{0}', processId: '{1}' on machine '{2}'.", process.ProcessName, process.Id, ExpressApp.MachineName);
                    Log("CommandLine: '{0}'.", Environment.CommandLine);
                    Log("Current user: '{0}', Domain: '{1}'.", Environment.UserName, Environment.UserDomainName);
                    Log("CurrentDirectory: '{0}'.", Environment.CurrentDirectory);

                    var domain = AppDomain.CurrentDomain;
                    Log("Domain BaseDirectory: '{0}', RelativeSearchPath: '{1}'", domain.BaseDirectory, domain.RelativeSearchPath);

                    var commonAssembly = typeof(AppLogger).Assembly;
                    Log("Assembly '{0}' has CodeBase: '{1}', Location: '{2}'", commonAssembly.FullName, commonAssembly.CodeBase, commonAssembly.Location);

                    domain.UnhandledException += Domain_UnhandledException;
                    domain.AssemblyResolve += Domain_AssemblyResolve;
                    domain.AssemblyLoad += Domain_AssemblyLoad;

                    foreach (var assembly in domain.GetAssemblies())
                    {
                        Log("Domain Assembly: '{0}'. Location: '{1}'", assembly.FullName, assembly.Location);
                    }
                }
                catch (Exception ex)
                {
                    Log("Failed AppLogger: {0}", ex);
                }
            }

            private void Domain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
            {
                Log("Domain AssemblyLoad for '{0}'. Location: '{1}'", args.LoadedAssembly.FullName, args.LoadedAssembly.Location);
            }

            private System.Reflection.Assembly Domain_AssemblyResolve(object sender, ResolveEventArgs args)
            {
                Log("Domain AssemblyResolve for '{0}'", args.Name);
                return null;
            }

            private void Domain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
            {
                Log("Domain UnhandledException: {0}", e.ExceptionObject);
            }
        }

        public sealed class DebugLogWriter : ILogWriter
        {
            private readonly bool _debug;

            public DebugLogWriter(bool debug = false)
            {
                _debug = debug;
            }

            public void Write(string format, params object[] args)
            {
                if (_debug)
                {
                    Debug.WriteLine(string.Format(format, args));
                    Debug.Flush();
                }
            }
        }
    }
}
