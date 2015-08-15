using Epsilon.Logic.FSharp.GoogleGeocode;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Epsilon.Web.Models.ViewModels.Admin
{
    public class TestGoogleGeocodeApiViewModel
    {
        [Required]
        [DataType(DataType.MultilineText)]
        public string Address { get; set; }

        [Required]
        public string Region { get; set; }
        public string Response { get; set; }

        public IList<Geometry> Geometries { get; set;}
    }
}
