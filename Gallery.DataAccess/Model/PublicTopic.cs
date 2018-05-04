using System.Collections.Generic;

namespace Gallery.Logic.Model
{
    public class PublicTopic
    {
        public string Title;
        public List<PublicBlob> Blobs = new List<PublicBlob>();
    }
}