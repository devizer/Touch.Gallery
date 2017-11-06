using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Gallery.MVC.API;
using Gallery.MVC.GalleryResources;
using Microsoft.AspNetCore.Mvc;
using Gallery.MVC.Models;
using Microsoft.Extensions.Logging;

namespace Gallery.MVC.Controllers
{
    public class HomeController : Controller
    {
        private ILogger<GalleryController> _Logger;
        private ContentManager _ContentManager;

        public HomeController(ILogger<GalleryController> logger, ContentManager contentManager)
        {
            _Logger = logger;
            _ContentManager = contentManager;
        }

        private const string DefaultTopic = "Kitty";


        [Route("/")]
        public IActionResult Index()
        {
            return View(new HomePageModel()
            {
                Topic = DefaultTopic,
            });
        }

        [Route("/{topic}")]
        public IActionResult Index(string topic)
        {
            var titles = _ContentManager.GetMetadata().SelectMany(x => x.Topics).Select(x => x.Title).Distinct();
            if (string.IsNullOrEmpty(topic)) topic = DefaultTopic;

            return View(new HomePageModel()
            {
                Topic = topic,
            });
        }



        [HttpPost]
        public IActionResult GetSliderHtml([FromForm] string galleryTitle, [FromForm] string limits, string ratio)
        {
            
            PublicLimits limitsParsed = PublicLimits.Parse(limits);
            List<PublicModel> meta = _ContentManager.GetMetadata();
            IEnumerable<PublicModel> byLimits = meta.Where(x => x.LimitValue == limitsParsed.LimitValue && x.Kind == limitsParsed.Kind);
            IEnumerable<PublicTopic> byTitle = byLimits.SelectMany(x => x.Topics).Where(x => x.Title == galleryTitle);
            PublicTopic foundGallery = byTitle.FirstOrDefault();
            if (foundGallery == null)
                throw new ArgumentException($"Gallery {galleryTitle} with specified limits ({limitsParsed}) not found");

            decimal ratioParsed;
            if (!decimal.TryParse(ratio, out ratioParsed))
                ratioParsed = 0;

            if (ratioParsed < 1) ratioParsed = 1;



            return PartialView("GalleryPartial", new PartialGalleryModel()
            {
                Limits = limitsParsed,
                Topic = foundGallery,
                Ratio = ratioParsed,
            });
        }

        public IActionResult Debug()
        {
            return View("Debug");
        }


        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }


}
