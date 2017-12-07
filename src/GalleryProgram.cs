using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            var host = BuildWebHost(args);
            try
            {
                var addresses = host.ServerFeatures.Get<IServerAddressesFeature>().Addresses;
                Addresses.AddRange(addresses.Select(x => x.Replace("://+", "://localhost").Replace("://0.0.0.0", "://localhost")));
            }
            catch(Exception ex)
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
    }
}
