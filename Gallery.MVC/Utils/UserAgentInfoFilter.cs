using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace Gallery.Prepare
{
    public class UserAgentInfoFilter : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var controller = filterContext.Controller as Controller;
            if (controller != null)
            {
                dynamic viewBag = controller.ViewBag;
                StringValues userAgent = filterContext.HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
                UserAgentInfo uaInfo = new UserAgentInfo(userAgent);
                controller.ViewBag.IsMobile = uaInfo.IsMobile;
                Trace.WriteLine("USER-AGENT: " + userAgent);
            }
        }


    }
}