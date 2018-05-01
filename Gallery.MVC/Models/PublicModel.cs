using System.Collections.Generic;

namespace Gallery.MVC.Models
{
    public class PublicModel
    {
        public LimitKind Kind;
        public int LimitValue;
        public List<PublicTopic> Topics = new List<PublicTopic>();
    }
}
