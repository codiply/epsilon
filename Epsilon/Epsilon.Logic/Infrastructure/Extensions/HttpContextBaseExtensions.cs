using Epsilon.Logic.Constants;
using System.Web;

namespace Epsilon.Logic.Infrastructure.Extensions
{
    public static class HttpContextBaseExtensions
    {
        public static void SetSanitizedIpAddress(this HttpContextBase context, string ip)
        {
            context.Items[HttpContextItemsKey.SANITIZED_IP_ADDRESS] = ip;
        }

        public static string GetSanitizedIpAddress(this HttpContextBase context)
        {
            return (string)context.Items[HttpContextItemsKey.SANITIZED_IP_ADDRESS];
        }
    }
}
