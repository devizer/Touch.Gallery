using System;
using System.Linq;
using Gallery.Logic.DataAccess;
using Gallery.MVC.GalleryResources;
using Gallery.MVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace Gallery.MVC.API
{
    [Produces("application/json")]
    [Route(ApiVersions.V1 + "/[controller]")]
    public class ActionController : Controller
    {
        private readonly PhotosRepository _photosRepository;
        private readonly ContentManager _ContentManager;

        public ActionController(PhotosRepository photosRepository, ContentManager contentManager)
        {
            _photosRepository = photosRepository;
            _ContentManager = contentManager;
        }

        [HttpPost]
        [Route("{userAction}/{idPhoto}")]
        public IActionResult Act(string userAction, string idPhoto)
        {

            if (!Enum.TryParse(typeof(UserAction), userAction, true, out var aRaw))
                throw new ArgumentException($"Action '{userAction}' is not support", nameof(userAction));

            UserAction a = (UserAction) aRaw;
            var topic = _ContentManager.GetMetadata().First().Topics.FirstOrDefault(t => t.Blobs.Any(b => b.IdContent == idPhoto));
            if (topic == null)
                throw new ArgumentException($"Photo '{userAction}' not found", nameof(idPhoto));

            string idUser = User?.Identity?.Name;
            if (string.IsNullOrEmpty(idUser))
                throw new InvalidOperationException("Action requires authenticated user");

            _photosRepository.AddUserAction(topic.Title, idUser, idPhoto, a);
            UserPhoto userPhoto = _photosRepository.GetUserPhoto(topic.Title, idUser, idPhoto);
            var photo = _photosRepository.GetContent(topic.Title, idPhoto);

            JsPhotoModel ret = new JsPhotoModel()
            {
                Id = userPhoto.IdContent,
                Url = "Not applicable",
                Height = -1,
                Width = -1,
                MyDislikes = userPhoto.Dislikes,
                MyLikes = userPhoto.Likes,
                MyShares = userPhoto.Shares,
                MyStars = userPhoto.Stars,
                TotalDislikes = photo.Dislikes,
                TotalLikes = photo.Likes,
                TotalShares = photo.Shares,
                TotalStars = photo.Stars,
            };

            return Ok(ret);
        }
    }
}
