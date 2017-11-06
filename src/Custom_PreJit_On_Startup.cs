using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Gallery.MVC.GalleryResources;
using Microsoft.Extensions.Logging;

namespace Gallery.MVC
{
    public class Custom_PreJit_On_Startup
    {
        private ILoggerFactory _LoggerFactory;
        private ILogger _StartUpLogger;
        private ContentManager _RM;

        public Custom_PreJit_On_Startup(ILoggerFactory loggerFactory, ContentManager rm)
        {
            _LoggerFactory = loggerFactory;
            _StartUpLogger = _LoggerFactory.CreateLogger("Custom Startup");
            _RM = rm;
        }

        public void Perform()
        {
            _StartUpLogger.LogInformation(@"Touch Gallery Environment:
 * Framefork: {0}
 * OS Architecture: {1}
 * OS: {2}
 * CPU Architecture: {3}",
                RuntimeInformation.FrameworkDescription,
                RuntimeInformation.OSArchitecture,
                RuntimeInformation.OSDescription,
                RuntimeInformation.ProcessArchitecture);

            var metaData = _RM.GetMetadata();
            var topics = metaData.SelectMany(x => x.Topics).Select(x => x.Title).Distinct().OrderBy(x => x).ToList();

            // read smallest blob
            var smallest = metaData.SelectMany(x => x.Topics).SelectMany(x => x.Blobs).OrderBy(x => x.Length).FirstOrDefault();
            if (smallest == null)
                throw new Exception("Any blob is not found");

            using (Stream s = _RM.GetBlobAsStream(smallest.Id))
            {
                s.CopyTo(Stream.Null);
            }

            GC.Collect(); GC.WaitForPendingFinalizers();
            GC.Collect(); GC.WaitForPendingFinalizers();


            _StartUpLogger.LogInformation(
                "Metadata: {0} Sizes, {2} Topics [{3}], Mem: {4:f0}",
                metaData.Count, topics.Count,
                string.Join(", ", topics),
                Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024
            );

        }
    }
}