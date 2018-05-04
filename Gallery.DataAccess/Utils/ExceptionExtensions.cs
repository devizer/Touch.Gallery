using System;
using System.Collections.Generic;

namespace Gallery.MVC.Utils
{
    public static class ExceptionExtensions
    {
        public static string GetExceptionDigest(this Exception ex)
        {
            List<string> ret = new List<string>();
            while (ex != null)
            {
                ret.Add(string.Format("[{0}] {1}", ex.GetType().Name, ex.Message));
                ex = ex.InnerException;
            }

            ret.Reverse();
            return string.Join(" <--- ", ret);
        }
    }
}
