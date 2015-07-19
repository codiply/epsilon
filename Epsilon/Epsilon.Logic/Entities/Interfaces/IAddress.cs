﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities.Interfaces
{
    public interface IAddress
    {
        string Line1 { get; }
        string Line2 { get; }
        string Line3 { get; }
        string Line4 { get; }
        string Locality { get; }
        string Region { get; }
        string Postcode { get; }
        string CountryId { get; }
    }

    public static class IAddressExtensions
    {
        public static string FullAddressWithoutCountry(this IAddress address)
        {
            var sb = new StringBuilder();
            var pieces = new List<string> {
                address.Line1, address.Line2, address.Line3, address.Line4,
                address.Locality, address.Region, address.Postcode };

            return string.Join(", ", pieces.Where(x => !string.IsNullOrWhiteSpace(x)));
        }
    }

}