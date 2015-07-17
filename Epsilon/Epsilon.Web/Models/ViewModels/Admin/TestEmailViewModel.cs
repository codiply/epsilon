using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Epsilon.Web.Models.ViewModels.Admin
{
    public class TestEmailViewModel
    {
        [Display(Name = "Recipient Display Name")]
        public string ToDisplayName { get; set; }

        [Display(Name = "Recipient Email address")]
        [Required]
        [DataType(DataType.EmailAddress)]
        public string ToEmailAddress { get; set; }
        
        [Required]
        public string Subject { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Body { get; set; }
    }
}