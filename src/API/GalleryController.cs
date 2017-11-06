using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Gallery.MVC.GalleryResources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Gallery.MVC.API
{
    [Route(ApiVersions.V1 + "/[controller]")]
    public class GalleryController : Controller
    {
        private ILogger<GalleryController> _Logger;
        private ContentManager _ContentManager;

        public GalleryController(ILogger<GalleryController> logger, ContentManager contentManager)
        {
            _Logger = logger;
            _ContentManager = contentManager;
        }

        [Route("Test777")]
        public IActionResult Get()
        {
            var file = @"V:\Gallery\src\History\BE050386.jpg";

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octec-stream");

            return File(stream, "image/jpeg");

            // return result;
        }

        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);
        static string GetSHA1(string arg)
        {
            if (arg == null)
                throw new ArgumentNullException("arg");

            var sha1 = System.Security.Cryptography.SHA1.Create();
            var hash = string.Join("", sha1.ComputeHash(Utf8.GetBytes(arg)).Select(x => x.ToString("X2")));
            return hash;
        }

        [Route("{id}")]
        public IActionResult GetBlob(string id)
        {
            var stream = _ContentManager.GetBlobAsStream(id);
            var tag = GetSHA1(id + _ContentManager.LastModified);
            return File(stream, "image/jpeg", _ContentManager.LastModified, new EntityTagHeaderValue(tag));
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
