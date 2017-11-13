using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gallery.MVC
{
    public class GalleryProgram
    {
        static readonly Stopwatch StartAt = Stopwatch.StartNew();
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);
            // host.Run();
            host.Start();
            Console.WriteLine("Start Time: " + StartAt.Elapsed + Environment.NewLine);
            Console.ReadLine();
            host.StopAsync().Wait();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
        }
    }
}
