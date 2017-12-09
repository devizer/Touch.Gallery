using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Gallery.MVC.Utils;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gallery.MVC
{
    public class GalleryProgram
    {
        public static readonly Stopwatch StartAt = Stopwatch.StartNew();
        public static readonly List<string> Addresses = new List<string>();

        public static void Main(string[] args)
        {
            CreatePidFile(GetDefaultPidFileFullPath());
            var host = BuildWebHost(args);
            try
            {
                var addresses = host.ServerFeatures.Get<IServerAddressesFeature>().Addresses;
                Addresses.AddRange(addresses.Select(x => x.Replace("://+", "://localhost").Replace("://0.0.0.0", "://localhost")));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Oops: " + ex.GetExceptionDigest());
            }

            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
            {
                Console.WriteLine("Server is shutting down");
            };

            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
        }

        static string GetDefaultPidFileFullPath()
        {
            const string key = "touch-galleries";
            string[] candidates = new[] {$"/var/run/{key}.pid", $"/tmp/{key}.pid"};
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    candidates = new[]
                    {
                        Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            $"run\\{key}.pid")
                    };
            }
            catch
            {
            }

            string pidFile = candidates.First();
            // TODO: Do we need check for access permission?
            return pidFile;
        }

        static bool CreatePidFile(string fullPath)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            }
            catch
            {
            }

            try
            {
                using (FileStream fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                using (StreamWriter wr = new StreamWriter(fs, Encoding.ASCII))
                {
                    wr.WriteLine(Process.GetCurrentProcess().Id);
                    Console.WriteLine($"PID file {fullPath} created");
                    return true;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Warning: Unable to create PID File '{fullPath}'. {ex.GetExceptionDigest()}");
                return false;
            }
        }


    }
}
