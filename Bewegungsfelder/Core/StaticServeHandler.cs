using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bewegungsfelder.Core
{
    class StaticServeHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                string path = request.RequestUri.LocalPath;

                if (path == "/")
                    path = "/index.html";

                //remove root
                path = path.TrimStart('/');

                // convert path separators from url to fs
                string fspath = path.Replace('/', Path.DirectorySeparatorChar);

                string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string page = Path.Combine(dir, "html", fspath);

                if (!File.Exists(page))
                {
                    return request.CreateErrorResponse(System.Net.HttpStatusCode.NotFound, $"{page} not found");
                }
                else
                {
                    using (StreamReader stream = new StreamReader(page))
                    {
                        var content = new StringContent(stream.ReadToEnd(), Encoding.UTF8, "text/html");
                        var response = request.CreateResponse(System.Net.HttpStatusCode.OK, content);
                        response.Content = content;
                        return response;
                    }
                }
            });
        }
    }
}
