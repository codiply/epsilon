using Epsilon.Logic.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Epsilon.Logic.Helpers
{
    public class IpAddressHelper : IIpAddressHelper
    {
        public string GetClientIpAddress(HttpRequestBase request)
        {
            try
            {
                var userHostAddress = request.UserHostAddress;

                // Attempt to parse. If it fails, we catch below and return "0.0.0.0".
                // Could use TryParse instead, but I wanted to catch all exceptions
                var parsedUserHostAddress = IPAddress.Parse(userHostAddress);

                var xForwardedFor = request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(xForwardedFor))
                    return parsedUserHostAddress.ToString();

                // Get a list of public ip addresses in the X_FORWARDED_FOR variable
                var publicForwardingIps = 
                    xForwardedFor.Split(',')
                    .Select(x => IPAddress.Parse(x))
                    .Where(x => !IsPrivateIpAddress(x)).ToList();

                // If we found any, return the last one, otherwise return the user host address
                return publicForwardingIps.Any() 
                    ? publicForwardingIps.Last().ToString()
                    : parsedUserHostAddress.ToString();
            }
            catch (Exception)
            {
                return "0.0.0.0";
            }
        }

        private bool IsPrivateIpAddress(IPAddress ip)
        {
            switch (ip.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    return IsPrivateIpAddressIPv4(ip);
                case AddressFamily.InterNetworkV6:
                    return IsPrivateIpAddressIPv6(ip);
                default:
                    return true;
            }
        }

        private bool IsPrivateIpAddressIPv4(IPAddress ipAddress)
        {
            // http://en.wikipedia.org/wiki/Private_network
            // Private IP Addresses are: 
            //  24-bit block: 10.0.0.0 through 10.255.255.255
            //  20-bit block: 172.16.0.0 through 172.31.255.255
            //  16-bit block: 192.168.0.0 through 192.168.255.255
            //  Link-local addresses: 169.254.0.0 through 169.254.255.255 (http://en.wikipedia.org/wiki/Link-local_address)

            var octets = ipAddress.GetAddressBytes();

            var is24BitBlock = octets[0] == 10;
            if (is24BitBlock)
                return true;

            var is20BitBlock = octets[0] == 172 && octets[1] >= 16 && octets[1] <= 31;
            if (is20BitBlock)
                return true;

            var is16BitBlock = octets[0] == 192 && octets[1] == 168;
            if (is16BitBlock)
                return true;

            var isLinkLocalAddress = octets[0] == 169 && octets[1] == 254;

            return isLinkLocalAddress;
        }

        private bool IsPrivateIpAddressIPv6(IPAddress ipAddress)
        {
            return ipAddress.IsIPv6LinkLocal || ipAddress.IsIPv6SiteLocal;
        }
    }
}
