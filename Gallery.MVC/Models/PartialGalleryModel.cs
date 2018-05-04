using System;
using System.Collections.Generic;
using Gallery.Logic.Model;

namespace Gallery.MVC.Models
{
    public class PartialGalleryModel
    {
        public PublicTopic Topic;
        public PublicLimits Limits;
        public decimal Ratio;

        [Obsolete("Moved to ViewBag.IsMobile", error: false)]
        public bool IsMobile;

        // Serialized List of JsPhotoModel
        public string AngularModel;
    }
}
