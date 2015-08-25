using Epsilon.Logic.Entities;
using Epsilon.Logic.Helpers.Interfaces;
using System.Collections.Generic;

namespace Epsilon.Web.Models.ViewModels.TextResource
{
    public class ListViewModel
    {
        public IList<LocalizedResource> Resources { get; set; }
        public string CultureCode { get; set; }
        public IList<Language> Languages { get; set; }
    }
}