using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gallery.MVC.Utils
{
    public class HashExtentions
    {
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

        public static string GetSHA1AsString(string arg)
        {
            if (arg == null)
                throw new ArgumentNullException("arg");

            var sha1 = System.Security.Cryptography.SHA1.Create();
            var hash = string.Join("", sha1.ComputeHash(Utf8.GetBytes(arg)).Select(x => x.ToString("X2")));
            return hash;
        }

        public static int GetSHA1AsSeed(string arg)
        {
            if (arg == null)
                throw new ArgumentNullException("arg");

            var sha1 = System.Security.Cryptography.SHA1.Create();
            var bytes = sha1.ComputeHash(Utf8.GetBytes(arg));
            int ret = 42;
            for (int i = 0; i < bytes.Length - 1 - 4; i++)
            {
                var next = BitConverter.ToInt32(bytes, i);
                ret = ret ^ next;
            }
            return ret;
        }



    }
}
