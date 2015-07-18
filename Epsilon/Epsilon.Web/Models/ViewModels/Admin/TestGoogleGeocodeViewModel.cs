using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epsilon.Logic.FSharp;

namespace Epsilon.Web.Models.ViewModels.Admin
{
    public class TestGoogleGeocodeViewModel
    {
        [Required]
        [DataType(DataType.MultilineText)]
        public string Address { get; set; }

        [Required]
        public string Region { get; set; }
        public string Response { get; set; }

        public Geocode ParsedResponse { get; set;}
    }
}
