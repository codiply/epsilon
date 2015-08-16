using Epsilon.Logic.Entities;
using Epsilon.Logic.Services.Interfaces;
using System;

namespace Epsilon.Logic.Models
{
    public class ViewPropertyInfoPropertySubmissionModel
    {
        public virtual Decimal? RentPerMonth { get; set; }
        public virtual string CurrencySymbol { get; set; }
        public virtual byte? NumberOfBedrooms { get; set; }
        public virtual bool? IsPartOfProperty { get; set; }
        public virtual bool? IsFurnished { get; set; }
        public virtual byte? LandlordRating { get; set; }
        public virtual byte? PropertyConditionRating { get; set; }
        public virtual byte? NeighboursRating { get; set; }
        public virtual DateTimeOffset? SubmittedOn { get; set; }

        public static ViewPropertyInfoPropertySubmissionModel FromTenancyDetailsSubmission(
            TenancyDetailsSubmission entity, ICurrencyService currencyService)
        {
            return new ViewPropertyInfoPropertySubmissionModel
            {
                RentPerMonth = entity.RentPerMonth,
                CurrencySymbol = currencyService.GetSymbol(entity.CurrencyId),
                NumberOfBedrooms = entity.NumberOfBedrooms,
                IsPartOfProperty = entity.IsPartOfProperty,
                IsFurnished = entity.IsFurnished,
                LandlordRating = entity.LandlordRating,
                PropertyConditionRating = entity.PropertyConditionRating,
                NeighboursRating = entity.NeighboursRating,
                SubmittedOn = entity.SubmittedOn
            };
        }
    }
}
