using Epsilon.Logic.Entities;
using Epsilon.Logic.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Epsilon.Web.Models.ViewModels.TextResource
{
    public class ListViewModel
    {
        public IList<LocalizedResource> Resources { get; set; }
        public string LanguageId { get; set; }
        public IList<Language> Languages { get; set; }
    }
}