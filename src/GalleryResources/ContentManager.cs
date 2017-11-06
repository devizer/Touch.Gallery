using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Gallery.MVC.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Gallery.MVC.GalleryResources
{
    public class ContentManager
    {
        private ILogger _Logger;

        private static List<PublicModel> _Metadata;
        static readonly object SyncMetadata = new object();
        private static string _BlobBasePath;
        private static DateTimeOffset _LastModified = new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero);

        public DateTimeOffset LastModified => _LastModified;

        public ContentManager(ILogger<ContentManager> logger)
        {
            _Logger = logger;
        }

        public Stream GetBlobAsStream(string idBlob)
        {
            var meta = GetMetadata();
            var found = meta
                .SelectMany(x => x.Topics).SelectMany(x => x.Blobs)
                .Where(x => x.Id == idBlob)
                .FirstOrDefault();

            if (found == null)
                throw new ArgumentException("Specified photo is not found");

            var name = _BlobBasePath + found.File.ToString("0", new CultureInfo("en-US"));
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            {
                if (s == null)
                    throw new ArgumentException($"Underlying stream {0} not found");

                s.Position = found.Position;
                MemoryStream mem = new MemoryStream();
                int l = 0;
                var buffer = new byte[32767];
                while (l < found.Length)
                {
                    var portion = Math.Min(found.Length - l, buffer.Length);
                    int n = s.Read(buffer, 0, portion);
                    if (n == 0) break;
                    l += n;
                    mem.Write(buffer, 0, n);
                }

                mem.Position = 0;
                return mem;
            }

        }



        public List<PublicModel> GetMetadata()
        {
            if (_Metadata != null) return _Metadata;
            lock (SyncMetadata)
            {
                if (_Metadata != null) return _Metadata;

                var ret = GetMetadata_Impl();
                _Metadata = ret;
                return ret;
            }
        }

        private List<PublicModel> GetMetadata_Impl()
        {
            var asmLocation = Assembly.GetExecutingAssembly().Location;
            var lastModified = new FileInfo(asmLocation).LastWriteTimeUtc;
            _LastModified = new DateTimeOffset(lastModified);
            _Logger.LogInformation("Global Last Modified: {0}", _LastModified);

            var ret = new List<PublicModel>();
            var names = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            _Logger.LogInformation("Embedded Resources: {0}{1}", Environment.NewLine,
                string.Join(Environment.NewLine, names.Select(x => $"  * [{x}]")));

            List<string> jsonNames = new List<string>();
            foreach (var name in names)
            {
                string[] parts = name.Split('.');
                if (parts.Length < 3 || parts[parts.Length - 3] != "GalleryResources") continue;
                string pre = parts[parts.Length - 2];
                string last = parts[parts.Length - 1];

                if (Int32.TryParse(parts[parts.Length - 1], out _) && pre == "blob")
                    _BlobBasePath = string.Join(".", parts.Take(parts.Length - 1)) + ".";

                if (last.StartsWith("("))
                    jsonNames.Add(name);
            }

            _Logger.LogInformation("Base Blob Path: " + _BlobBasePath);
            _Logger.LogInformation("Galleries (paths): {0}{1}", Environment.NewLine,
                string.Join(Environment.NewLine, jsonNames.Select(x => $"  * [{x}]")));

            foreach (var jsonName in jsonNames)
            {
                using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(jsonName))
                using (StreamReader rdr = new StreamReader(s, Encoding.UTF8))
                {
                    var json = rdr.ReadToEnd();
                    JsonSerializer ser = new JsonSerializer();
                    JsonTextReader jr = new JsonTextReader(new StringReader(json));
                    var model = ser.Deserialize<PublicModel>(jr);
                    ret.Add(model);
                }
            }
            return ret;
        }
    }
}
