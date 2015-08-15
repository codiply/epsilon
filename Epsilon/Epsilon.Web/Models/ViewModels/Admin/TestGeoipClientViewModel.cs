using Epsilon.Logic.Models;
using System.ComponentModel.DataAnnotations;

namespace Epsilon.Web.Models.ViewModels.Admin
{
    public class TestGeoipClientViewModel
    {
        [Required]
        public string IpAddress { get; set; }

        public string GeoipProviderName { get; set; }

        public GeoipClientResponse ClientResponse { get; set;}
    }
}
