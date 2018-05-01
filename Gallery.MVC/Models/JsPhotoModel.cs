namespace Gallery.MVC.Models
{
    public class JsPhotoModel
    {
        public string Url;
        public int Width;
        public int Height;
        public string Id;

        public bool MyStars { get; set; }
        public bool MyLikes { get; set; }
        public bool MyDislikes { get; set; }
        public bool MyShares { get; set; }

        public long TotalStars { get; set; }
        public long TotalLikes { get; set; }
        public long TotalDislikes { get; set; }
        public long TotalShares { get; set; }
    }
}