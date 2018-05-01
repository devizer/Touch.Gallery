using System.Collections.Generic;

namespace Gallery.MVC.Models
{
    public class PublicTopic
    {
        public string Title;
        public List<PublicBlob> Blobs = new List<PublicBlob>();
    }
}