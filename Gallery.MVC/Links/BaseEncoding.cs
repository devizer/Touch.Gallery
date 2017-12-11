using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gallery.MVC.Links
{
    public static class GuidExtentions
    {
        public static string ToShortString(this Guid guid)
        {
            Ascii85 a = new Ascii85() {EnforceMarks = false};
            var ret = a.Encode(guid.ToByteArray());
            return ret;
        }
    }
}
