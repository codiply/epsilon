using Epsilon.Logic.Entities;
using Epsilon.Logic.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Epsilon.Logic.Models
{
    public class ViewPropertyInfoModel
    {
        public ViewPropertyInfoPropertyModel MainProperty { get; set; }
        public IList<ViewPropertyInfoPropertyModel> DuplicateProperties { get; set; }

        /// <summary>
        /// NOTE: you will need to include Country and TenancyDetailsSubmissions on all addresses passed in for this to work.
        /// </summary>
        /// <param name="mainProperty"></param>
        /// <param name="duplicateProperties"></param>
        /// <returns></returns>
        public static ViewPropertyInfoModel Construct(Address mainProperty, IList<Address> duplicateProperties, ICurrencyService currencyService)
        {
            return new ViewPropertyInfoModel
            {
                MainProperty = ViewPropertyInfoPropertyModel.FromAddress(mainProperty, currencyService),
                DuplicateProperties = duplicateProperties.Select(x =>
                    ViewPropertyInfoPropertyModel.FromAddress(x, currencyService)).ToList()
            };
        }
    }
}
