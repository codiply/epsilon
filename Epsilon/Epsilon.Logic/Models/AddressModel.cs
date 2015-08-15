using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Entities.Interfaces;

namespace Epsilon.Logic.Models
{
    public class AddressModel
    {
        // !!! IMPORTANT !!!
        // If you add new fields, make sure you update method From() at the bottom

        public virtual string Line1 { get; set; }
        public virtual string Line2 { get; set; }
        public virtual string Line3 { get; set; }
        public virtual string Line4 { get; set; }
        public virtual string Locality { get; set; }
        public virtual string Region { get; set; }
        public virtual string Postcode { get; set; }
        public virtual string CountryId { get; set; }
        public virtual CountryId CountryIdAsEnum { get; set; }
        public virtual string CountryEnglishName { get; set; }
        public virtual string CountryLocalName { get; set; }

        /// <summary>
        /// Note: For this to fully work you need to Include the Country to the Address.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static AddressModel FromEntity(Address address)
        {
            // TODO_TEST_PANOS
            return new AddressModel
            {
                Line1 = address.Line1,
                Line2 = address.Line2,
                Line3 = address.Line3,
                Line4 = address.Line4,
                Locality = address.Locality,
                Region = address.Region,
                Postcode = address.Postcode,
                CountryId = address.CountryId,
                CountryIdAsEnum = address.CountryIdAsEnum(),
                CountryEnglishName = address.Country.EnglishName,
                CountryLocalName = address.Country.LocalName
            };
        }
    }
}
