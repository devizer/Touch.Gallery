namespace Gallery.MVC.DataAccess
{
    public class UserPhoto
    {
        public string Topic { get; set; }
        public string IdContent { get; set; }
        public string IdUser { get; set; }
        public bool Stars { get; set; }
        public bool Likes { get; set; }
        public bool Dislikes { get; set; }
        public bool Shares { get; set; }

        public override string ToString()
        {
            return $"{nameof(Topic)}: {Topic}, {nameof(IdContent)}: {IdContent}, {nameof(IdUser)}: {IdUser}, {nameof(Stars)}: {Stars}, {nameof(Likes)}: {Likes}, {nameof(Dislikes)}: {Dislikes}, {nameof(Shares)}: {Shares}";
        }
    }

    public class Content
    {
        public string Topic { get; set; }
        public string IdContent { get; set; }
        public long Stars { get; set; }
        public long Likes { get; set; }
        public long Dislikes { get; set; }
        public long Shares { get; set; }
    }
}
