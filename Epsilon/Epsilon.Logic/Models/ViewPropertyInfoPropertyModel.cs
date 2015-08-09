using Epsilon.Logic.Entities;
using Epsilon.Logic.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Models
{
    public class ViewPropertyInfoPropertyModel
    {
        public string DisplayAddress { get; set; }

        public IList<ViewPropertyInfoPropertySubmissionModel> CompleteSubmissions { get; set; }

        /// <summary>
        /// NOTE: You will need to Include Address.Country, Address.TenancyDetailsSubmissions and
        ///       Address.TenancyDetailsSubmissions.Select(x => x.Currency) for this to work.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static ViewPropertyInfoPropertyModel FromAddress(Address entity, ICurrencyService currencyService)
        {
            // TODO_PANOS_TEST
            var completeSubmissions = entity.TenancyDetailsSubmissions
                .Where(s => s.SubmittedOn.HasValue)
                .Select(s => ViewPropertyInfoPropertySubmissionModel.FromTenancyDetailsSubmission(s, currencyService))
                .ToList();

            return new ViewPropertyInfoPropertyModel
            {
                DisplayAddress = entity.FullAddress(),
                CompleteSubmissions = completeSubmissions
            };
        }
    }
}
