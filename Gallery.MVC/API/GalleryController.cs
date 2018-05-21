using System.IO;
using System.Net;
using System.Net.Http;
using Gallery.MVC.GalleryResources;
using Gallery.MVC.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        }


        [Route("{id}")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 24*3600*365)]
        public IActionResult GetBlob(string id)
        {
            var stream = _ContentManager.GetBlobAsStream(id);
            var tag = HashExtentions.GetSHA1AsString(id + _ContentManager.LastModified);
            // HttpContext.Response.Headers.Add("Cache-control", "public, max-age=1314000");
            return File(
                stream,
                "image/jpeg"
                );
                // new EntityTagHeaderValue('"' + tag + '"'));
        }

    }
}
