using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Gallery.MVC.API;
using Gallery.MVC.GalleryResources;
using Gallery.MVC.Links;
using Microsoft.AspNetCore.Mvc;
using Gallery.MVC.Models;
using Gallery.MVC.Utils;
using Gallery.Prepare;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Gallery.MVC.Controllers
{
    [UserAgentInfoFilter]
    public class HomeController : Controller
    {
        private ILogger<GalleryController> _Logger;
        private ContentManager _ContentManager;

        public HomeController(ILogger<GalleryController> logger, ContentManager contentManager)
        {
            _Logger = logger;
            _ContentManager = contentManager;
        }


        [Route("/")]
        public async Task<IActionResult> Index()
        {
            AssingIdToBrowser();
            return View(new HomePageModel()
            {
                Topic = HomePageModel.DefaultTopic,
            });
        }

        [Route("/{topic}")]
        public IActionResult Index(string topic)
        {
            AssingIdToBrowser();
            var titles = _ContentManager.GetMetadata().SelectMany(x => x.Topics).Select(x => x.Title).Distinct();
            if (string.IsNullOrEmpty(topic)) topic = HomePageModel.DefaultTopic;

            return View(new HomePageModel()
            {
                Topic = topic,
            });
        }



        [HttpPost]
        public IActionResult GetSliderHtml([FromForm] string galleryTitle, [FromForm] string limits, [FromForm] string ratio)
        {
            _Logger.LogDebug("User1: " + HttpContext.User?.Identity?.Name
                + Environment.NewLine + "User2: " + User.Identity.Name
            );


            PublicLimits limitsParsed = PublicLimits.Parse(limits);
            List<PublicModel> meta = _ContentManager.GetMetadata();
            IEnumerable<PublicModel> byLimits = meta.Where(x => x.LimitValue == limitsParsed.LimitValue && x.Kind == limitsParsed.Kind);
            IEnumerable<PublicTopic> byTitle = byLimits.SelectMany(x => x.Topics).Where(x => x.Title == galleryTitle);
            PublicTopic foundGallery = byTitle.FirstOrDefault();
            if (foundGallery == null)
                throw new ArgumentException($"Gallery [{galleryTitle}] with specified limits ({limitsParsed}) not found");

            decimal ratioParsed;
            if (!decimal.TryParse(ratio, out ratioParsed))
                ratioParsed = 0;

            if (ratioParsed < 1) ratioParsed = 1;


            // Shuffle gallery based on remote ip
            var galleryCopy = new PublicTopic()
            {
                Title = foundGallery.Title,
                Blobs = new List<PublicBlob>(foundGallery.Blobs)
            };
            var seedByIp = HashExtentions.GetSHA1AsSeed(HttpContext.Connection.RemoteIpAddress.ToString());
            galleryCopy.Blobs.Shuffle(seedByIp);

            var userAgent = HttpContext.Request.Headers["User-Agent"];
            var uaInfo = new UserAgentInfo(userAgent);

            List<JsPhotoModel> jsPhotos =
                galleryCopy.Blobs.Select(x => new JsPhotoModel()
                {
                    Id = x.IdContent,
                    Url = x.Id,
                    Height = x.Height,
                    Width = x.Width
                }).ToList();

            if (jsPhotos.Any())
            {
                jsPhotos[0].TotalLikes = 42;
                jsPhotos[0].TotalDislikes = 1;
                jsPhotos[0].TotalShares = 2;
                jsPhotos[0].TotalStars = 7777;
                jsPhotos[0].MyLikes = true;
                jsPhotos[0].MyDislikes = true;
                jsPhotos[0].MyStars = true;
                jsPhotos[0].MyShares = true;
            }

            var angularModel = new
            {
                Gallery = galleryCopy.Title,
                Ratio = ratioParsed,
                Limits = limitsParsed,
                IsMobile = uaInfo.IsMobile,
                Photos = jsPhotos,
            };

            return PartialView("GalleryPartial", new PartialGalleryModel()
            {
                Limits = limitsParsed,
                Topic = galleryCopy,
                Ratio = ratioParsed,
                IsMobile = uaInfo.IsMobile,
                AngularModel = angularModel.ToNewtonJSon(TheAppContext.IsDebug)
            });
        }


        [HttpPost]
        public IActionResult GetSmartSliderHtml([FromForm] string galleryTitle, [FromForm] string windowHeight, [FromForm] string devicePixelRatio)
        {
            var enUs = new CultureInfo("en-US");

            decimal argHeight = 0;
            decimal.TryParse(windowHeight, NumberStyles.AllowDecimalPoint, enUs, out argHeight);

            decimal argRatio = 0;
            decimal.TryParse(devicePixelRatio, NumberStyles.AllowDecimalPoint, enUs, out argRatio);


            PublicLimits targetLimit = null;
            decimal targetRatio = 1;

            targetLimit = new PublicLimits(LimitKind.Height, 512);
            targetRatio = 1.0m;

            var userAgent = HttpContext.Request.Headers["User-Agent"];
            var uaInfo = new UserAgentInfo(userAgent);

            var reveredLimits = new List<PublicLimits>(TheAppContext.Limits);
            reveredLimits.Reverse();

            _Logger.LogInformation("Sorted Limits: " + string.Join(" ", reveredLimits));

            targetRatio = argRatio;
            var pixels = argHeight * argRatio;
            PublicLimits foundLimits = reveredLimits.FirstOrDefault(x => x.Kind == LimitKind.Height && x.LimitValue <= pixels);
            var found0 = foundLimits; 
            if (foundLimits == null)
            {
                if (!uaInfo.IsMobile)
                {
                    foundLimits = new PublicLimits(LimitKind.Height, 672);
                }
                else
                {
                    foundLimits = new PublicLimits(LimitKind.Height, 288);
                }
            }

            _Logger.LogInformation(string.Format(
                @"         Agent: {0}{1}Family+Version: {2}{1}   Height(arg): {3}{1}    Ratio(arg): {4}{1}        found0: {5}{1}      SELECTED: {6} / {7}{1}     Remote IP: {8}",
                userAgent,
                Environment.NewLine,
                uaInfo + ", " + (uaInfo.IsMobile ? "Mobile" : "PC"),
                argHeight,
                argRatio,
                found0 + " / " + targetRatio,
                foundLimits, targetRatio,
                HttpContext.Connection.RemoteIpAddress
            ));

            return GetSliderHtml(
                galleryTitle,
                foundLimits.Serialize(),
                targetRatio.ToString("f3", enUs)
            );

        }

        public IActionResult Debug()
        {
            return View("Debug");
        }


        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        async void AssingIdToBrowser()
        {
            var user = User?.Identity?.Name;
            if (!string.IsNullOrEmpty(user))
                return;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Browser-" + Guid.NewGuid().ToShortString()),
            };

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                }).Wait();
        }
    }


}

