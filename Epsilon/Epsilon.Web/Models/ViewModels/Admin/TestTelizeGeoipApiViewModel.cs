using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epsilon.Logic.FSharp.TelizeGeoip;

namespace Epsilon.Web.Models.ViewModels.Admin
{
    public class TestTelizeGeoipApiViewModel
    {
        [Required]
        public string IpAddress { get; set; }

        public string Response { get; set; }

        public GeoipInfo GeoipInfo { get; set;}
    }
}
