﻿using System;
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

            PhotosRepository repo = new PhotosRepository();
            List<PublicTopic> topicsWithBlobs = metaData.First().Topics;

            Parallel.Invoke(
                () =>
                {
                    repo.CreateTopics(topics);
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

            Action[] actions = new Action[]
            {
                () => repo.AddUserAction("Tester", "Disliked-content", UserAction.Dislike),
                () => repo.AddUserAction("Tester", "Liked-content", UserAction.Like),
                () => repo.AddUserAction("Another Tester", "Liked-content", UserAction.Like),
                () => repo.AddUserAction("Tester", "Starred-content", UserAction.Star),
                () => repo.AddUserAction("Tester", "Shared-content", UserAction.Share),
                () => {
                    repo.AddUserAction("Tester", "Double-Liked-content", UserAction.Like);
                    repo.AddUserAction("Tester", "Double-Liked-content", UserAction.Like);
                },
                () => {
                    repo.AddUserAction("Tester", "Liked-and-then-Disliked-content", UserAction.Like);
                    repo.AddUserAction("Tester", "Liked-and-then-Disliked-content", UserAction.Dislike);
                },
                () => {
                    repo.AddUserAction("Tester", "Disliked-and-then-Liked-content", UserAction.Dislike);
                    repo.AddUserAction("Tester", "Disliked-and-then-Liked-content", UserAction.Like);
                },
            };

            Parallel.Invoke(actions);

            ThreadPool.QueueUserWorkItem(_ =>
            {
                Thread.Sleep(2000);
                var httpHost = "http://localhost:5000";
                if (GalleryProgram.Addresses.Any()) httpHost = GalleryProgram.Addresses.First();

                Stopwatch startAt = Stopwatch.StartNew();
                try
                {
                    HttpClient c = new HttpClient();
                    var bytes = c.GetByteArrayAsync(httpHost).Result;
                    _StartUpLogger.LogInformation($"Pre-JITed [{httpHost}] in {startAt.Elapsed}");
                }
                catch(Exception ex)
                {
                    _StartUpLogger.LogWarning($"Pre-JIT [{httpHost}] failed. " + ex.GetExceptionDigest());
                }
            });



        }
    }
}