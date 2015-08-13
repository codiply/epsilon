using Epsilon.Logic.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
