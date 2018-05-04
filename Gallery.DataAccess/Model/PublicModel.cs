using System.Collections.Generic;

namespace Gallery.Logic.Model
{
    public class PublicModel
    {
        public LimitKind Kind;
        public int LimitValue;
        public List<PublicTopic> Topics = new List<PublicTopic>();
    }
}
