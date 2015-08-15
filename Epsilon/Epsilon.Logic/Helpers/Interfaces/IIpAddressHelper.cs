using System.Web;

namespace Epsilon.Logic.Helpers.Interfaces
{
    public interface IIpAddressHelper
    {
        string GetClientIpAddress(HttpRequestBase request);
    }
}
