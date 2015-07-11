using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Epsilon.Logic.Helpers.RandomStringHelper;

namespace Epsilon.Logic.TestDataPopulation
{
    public static class GbAddressPopulator
    {
        private class Area
        {
            public string City { get; set; }
            public string County { get; set; }
            public string PostcodePrefix { get; set; }
        }

        private static string[] _streetNames =
            {
                "Isaac Newton",
                "Niels Bohr",
                "Galileo Galilei",
                "Albert Einstein",
                "James Clerk Maxwell",
                "Michael Faraday",
                "Marie Curie",
                "Richard Feynman",
                "Ernest Rutherford",
                "Paul Dirac"
            };

        private static string[] _streetSuffixes =
            {
                "Street", "Road", "Way", "Avenue", "Drive", "Grove", "Lane", "Gardens",
                "Place", "Crescent", "Close", "Square", "Hill", "Circus", "Court", "Yard"
            };

        private static string[] _houseNames =
            {
                "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta", "Theta",
                "Iota", "Kappa", "Lambda", "Mu", "Nu", "Xi", "Omicron", "Pi",
                "Rho", "Sigma", "Tau", "Upsilon", "Phi", "Chi", "Psi", "Omega"
            };

        private static string[] _houseType =
            {
                "House", "Lodge", "Cottages"
            };

        private static Area[] _areas =
            {
                new Area { City = "London", County = "London", PostcodePrefix = "W1" },
                new Area { City = "London", County = "London", PostcodePrefix = "E1" },
                new Area { City = "London", County = "London", PostcodePrefix = "N1" },
                new Area { City = "Oxford", County = "Oxfordshire", PostcodePrefix = "OX1" },
                new Area { City = "Oxford", County = "Oxfordshire", PostcodePrefix = "OX2" },
                new Area { City = "Oxford", County = "Oxfordshire", PostcodePrefix = "OX3" },
                new Area { City = "Cambridge", County = "Cambridgeshire", PostcodePrefix = "OX1" },
                new Area { City = "Cambridge", County = "Cambridgeshire", PostcodePrefix = "OX2" },
                new Area { City = "Cambridge", County = "Cambridgeshire", PostcodePrefix = "OX3" },
            };

        public static async Task Populate(
            IRandomWrapper random,
            IEpsilonContext dbContext, 
            string userId,
            int postCodesPerArea, 
            int housesPerPostcode,
            int minAddressesPerHouse,
            int maxAddressesPerHouse)
        {
            foreach (var area in _areas)
            {
                var postcodes = Enumerable.Range(0, postCodesPerArea).Select(x =>
                    string.Format("{0}{1}", area.PostcodePrefix, RandomPostcodeSuffix(random)));
                foreach (var postcode in postcodes)
                {
                    for (int i = 0; i < housesPerPostcode; i++)
                    {
                        var numberOfAddresses = random.Next(minAddressesPerHouse, maxAddressesPerHouse + 1);
                        var addressGroup = HouseAddressGroup(
                            userId,
                            numberOfAddresses,
                            RandomHouse(random),
                            RandomStreet(random),
                            area.City,
                            area.County,
                            postcode);
                        dbContext.Addresses.AddRange(addressGroup);
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
        }

        private static string RandomHouse(IRandomWrapper random)
        {
            return string.Format("{0} {1}",
                random.Pick(_houseNames), random.Pick(_houseType));
        }

        private static string RandomStreet(IRandomWrapper random)
        {
            return string.Format("{0} {1}",
                random.Pick(_streetNames), random.Pick(_streetSuffixes));
        }

        private static string RandomPostcodeSuffix(IRandomWrapper random)
        {
            var number = RandomStringHelper.GetDigit(random);
            var alpha = RandomStringHelper.GetString(random, 2, CharacterCase.Upper);
            return number + alpha;
        }

        private static IList<Address> HouseAddressGroup(
            string userId,
            int numberOfAddresses,
            string houseName,
            string street,
            string city,
            string county,
            string postcode)
        {
            return Enumerable.Range(1, numberOfAddresses).Select(i =>
                new Address
                {
                    Line1 = string.Format("{0} {1}", i, houseName),
                    Line2 = street,
                    Locality = city,
                    Region = county,
                    Postcode = postcode,
                    CountryId = EnumsHelper.CountryId.ToString(CountryId.GB),
                    CreatedById = userId
                }).ToList();
        }
    }
}
