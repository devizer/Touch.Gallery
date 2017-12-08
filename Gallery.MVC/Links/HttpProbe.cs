using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WaitFor.Common
{
    public class HttpProbe
    {
        public static async Task Go(HttpConnectionString cs, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cancellationToken == default(CancellationToken))
                cancellationToken = CancellationToken.None;

            HttpClient c = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(cs.Timeout),
            };

            HttpRequestMessage req = new HttpRequestMessage(
                new HttpMethod(cs.Method.ToUpper()), 
                new Uri(cs.Uri)
                );

            if (cs.Payload != null)
                req.Content = new StringContent(cs.Payload);

            var copy = new List<HttpConnectionString.Header>(cs.Headers.ToList());
            var contentType = copy.FirstOrDefault(x => "Content-Type".Equals(x.Name, StringComparison.InvariantCultureIgnoreCase));
            if (contentType != null)
            {
                req.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType.Values.First());
                copy.Remove(contentType);
            }

            foreach (var header in copy)
                req.Content.Headers.Add(header.Name, header.Values);

            // if (cs.ConnectionString.IndexOf("Smart") >= 0 && Debugger.IsAttached) Debugger.Break();

            var response = await c.SendAsync(req, cancellationToken);
            var statusCode = response.StatusCode;
            int statusInt = (int) statusCode;
            bool isValid = cs.ExpectedStatus.IsValid(statusInt);
            if (!isValid)
                throw new InvalidOperationException($"Returned status code {statusInt} does not conform expected value. Request: \"{cs.ConnectionString}\"");


        }

        private static readonly Dictionary<string, HttpMethod> MethodsByString = new Dictionary<string, HttpMethod>(StringComparer.InvariantCultureIgnoreCase)
        {
            {"Get", HttpMethod.Get},
            {"Put", HttpMethod.Put},
            {"Delete", HttpMethod.Delete},
            {"Head", HttpMethod.Head},
            {"Options", HttpMethod.Options},
            {"Post", HttpMethod.Post},
            {"Trace", HttpMethod.Trace},
        };

    }
}
