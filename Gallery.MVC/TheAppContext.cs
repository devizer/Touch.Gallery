using System;
using System.Collections.Generic;
using System.Linq;
using Gallery.MVC.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Gallery.MVC
{
    public class TheAppContext
    {
        public static readonly string AuthCookieScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        public static bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }


        public static bool IsTravisCI
        {
            get
            {
                var t = Environment.GetEnvironmentVariable("TRAVIS");
                return
                    !string.IsNullOrEmpty(t)
                    && "true".Equals(t, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        private static List<PublicLimits> _Limits;

        public static void AssignLimits(IEnumerable<PublicLimits> limits)
        {
            _Limits = new List<PublicLimits>(limits.OrderBy(x => x.Kind).ThenBy(x => x.LimitValue));
        }

        public static List<PublicLimits> Limits
        {
            get
            {
                if (_Limits == null)
                    throw new InvalidOperationException("App didn't initialized properly. It seems Custom_PreJit_On_Startup.Perform call was lost");

                return _Limits;
            }
        }
    }
}
