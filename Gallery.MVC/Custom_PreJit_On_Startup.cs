using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Gallery.MVC.DataAccess;
using Gallery.MVC.GalleryResources;
using Gallery.MVC.Models;
using Gallery.MVC.Utils;
using Microsoft.Extensions.Logging;
using WaitFor.Common;

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

            List<PublicModel> metaData = _RM.GetMetadata();
            var topics = metaData.SelectMany(x => x.Topics).Select(x => x.Title).Distinct().OrderBy(x => x).ToList();

            // read smallest blob
            PublicBlob smallest = metaData.SelectMany(x => x.Topics).SelectMany(x => x.Blobs).OrderBy(x => x.Length).FirstOrDefault();
            if (smallest == null)
                throw new Exception("Any blob is not found");

            using (Stream s = _RM.GetBlobAsStream(smallest.Id))
            {
                s.CopyTo(Stream.Null);
            }

            GC.Collect(); GC.WaitForPendingFinalizers();
            GC.Collect(); GC.WaitForPendingFinalizers();

            TheAppContext.AssignLimits(metaData
                .Select(x => new PublicLimits() {Kind = x.Kind, LimitValue = x.LimitValue}).Distinct()
            );

            Func<string, int> getBlobsByTopics = (topic) =>
            {
                return metaData.SelectMany(x => x.Topics).First(x => x.Title == topic).Blobs.Count();
            };

            _StartUpLogger.LogInformation(
                "Metadata: {0} Sizes, {2} Topics [{3}], Mem: {4:f0}",
                metaData.Count, topics.Count,
                string.Join(", ", topics.Select(x => $"{x}({getBlobsByTopics(x)})")),
                Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024
            );


            ThreadPool.QueueUserWorkItem(_ =>
            {
                Thread.Sleep(2000);
                BuiltInPreJIT(smallest.Id);

                Thread.Sleep(666);
                List<PublicTopic> topicsWithBlobs = metaData.First().Topics;
                BuiltInStorageTests(topicsWithBlobs);
            });


        }

        private static void BuiltInStorageTests(List<PublicTopic> firstTopics)
        {
            PhotosRepository repo = new PhotosRepository();

            var topicNames = firstTopics.Select(x => x.Title);
            Parallel.Invoke(
                () =>
                {
                    if (!TheAppContext.IsTravisCI)
                        repo.CreateTopics(topicNames);
                },
                () =>
                {
/*
                    Parallel.ForEach(topicsWithBlobs, (topicWithBlobs) =>
                    {
                        var idContentList = topicWithBlobs.Blobs.Select(x => x.IdContent).ToList();
                        repo.CreateContent(idContentList);
                        Console.WriteLine($"Saved {idContentList.Count} related blobs of Topic: {topicWithBlobs.Title}");
                    });
*/
                });

            var topics = new[] {"One Topic", "Another Topic"};
            var totalActions = new List<Action>();
            foreach (var topic_ in topics)
            {
                var topic = topic_;
                Action[] actions = new Action[]
                {
                    () => repo.AddUserAction(topic, "Tester", "Disliked-content", UserAction.Dislike),
                    () => repo.AddUserAction(topic, "Tester", "Liked-content", UserAction.Like),
                    () => repo.AddUserAction(topic, "Another Tester", "Liked-content", UserAction.Like),
                    () => repo.AddUserAction(topic, "Tester", "Starred-content", UserAction.Star),
                    () => repo.AddUserAction(topic, "Tester", "Shared-content", UserAction.Share),
                    () =>
                    {
                        repo.AddUserAction(topic, "Tester", "Double-Liked-content", UserAction.Like);
                        repo.AddUserAction(topic, "Tester", "Double-Liked-content", UserAction.Like);
                    },
                    () =>
                    {
                        repo.AddUserAction(topic, "Tester", "Liked-and-then-Disliked-content", UserAction.Like);
                        repo.AddUserAction(topic, "Tester", "Liked-and-then-Disliked-content", UserAction.Dislike);
                    },
                    () =>
                    {
                        repo.AddUserAction(topic, "Tester", "Disliked-and-then-Liked-content", UserAction.Dislike);
                        repo.AddUserAction(topic, "Tester", "Disliked-and-then-Liked-content", UserAction.Like);
                    },
                };

                totalActions.AddRange(actions);

            }

            var numThreads = totalActions.Count;
            ParallelOptions opts = new ParallelOptions() {MaxDegreeOfParallelism = numThreads };
            Parallel.ForEach(totalActions, opts, (a) =>
            {
                try
                {
                    a();
                }
                catch
                {
                }
            });

            repo.GetUserPhotosByTopic("One Topic", "Tester");
            Stopwatch sw = Stopwatch.StartNew();
            var byUser = repo.GetUserPhotosByTopic("One Topic", "Tester");
            Console.WriteLine($"Marks retrieved in {sw.Elapsed}:");
            foreach (var userPhoto in byUser)
            {
                Console.WriteLine("  " + userPhoto.Value.ToDebugString());
            }

        }

        private void BuiltInPreJIT(string smallestBlobId)
        {
            var urlBase = "http://localhost:5000";
            if (GalleryProgram.Addresses.Any()) urlBase = GalleryProgram.Addresses.First();
            urlBase = urlBase.TrimEnd('/');

            string[] reqs =
            {
                $"{urlBase}/; Method=Get; Valid Status = 100-299",

                $"Uri = {urlBase}/api/v1/Gallery/{smallestBlobId}; Method=Get; Valid Status = 100-299,403",

                $"{urlBase}/Home/GetSmartSliderHtml; Method=Post; Valid Status = 100-299,403;" +
                $" Payload=galleryTitle=Kitty&windowHeight=666&devicePixelRatio=1;" +
                $" *Accept = text/html;" +
                $" *Accept-Language = en-US, en;" +
                $" *Content-Type = application/x-www-form-urlencoded;" +
                $" *X-Requested-With = XMLHttpRequest",
            };

            Parallel.ForEach(reqs, (req) =>
            {
                try
                {
                    Stopwatch startAt = Stopwatch.StartNew();
                    HttpConnectionString cs = new HttpConnectionString(req);
                    HttpProbe.Go(cs).Wait();
                    _StartUpLogger.LogInformation($"Pre-JITed [{req}] in {startAt.Elapsed}");
                }
                catch(Exception ex)
                {
                    _StartUpLogger.LogWarning($"Pre-JIT [{req}] failed. " + ex.GetExceptionDigest());
                }

            });

            if (false)
            {
                Stopwatch startAt = Stopwatch.StartNew();
                try
                {
                    HttpClient c = new HttpClient();
                    var bytes = c.GetByteArrayAsync(urlBase).Result;
                    _StartUpLogger.LogInformation($"Pre-JITed [{urlBase}] in {startAt.Elapsed}");
                }
                catch (Exception ex)
                {
                    _StartUpLogger.LogWarning($"Pre-JIT [{urlBase}] failed. " + ex.GetExceptionDigest());
                }
            }
        }
    }
}