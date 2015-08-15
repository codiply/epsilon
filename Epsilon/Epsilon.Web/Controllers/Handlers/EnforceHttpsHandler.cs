using Epsilon.Logic.Constants;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Epsilon.Web.Controllers.Handlers
{
    public class EnforceHttpsHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (ConfigurationManager.AppSettings[AppSettingsKey.DisableHttps].ToLower().Equals("true"))
            {
                return base.SendAsync(request, cancellationToken);
            }

            // Enforce HTTPS
            if (request.RequestUri.Scheme != Uri.UriSchemeHttps)
            {
                return Task<HttpResponseMessage>.Factory.StartNew(
                    () =>
                    {
                        var response = new HttpResponseMessage(HttpStatusCode.Forbidden)
                        {
                            Content = new StringContent("HTTPS Required")
                        };

                        return response;
                    });
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
