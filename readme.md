## Development environment setup

### Visual studio

Install Visual Studio 2015 Community Edition. 

Choose **Custom** as the type of Installation and then install either all features, or at least the following:

- Microsoft SQL Server Data Tools
- Microsoft Web Developer Tools

From **Tools > Extension and Updates...** install

* **Typescript 1.5 for Visual Studio 2015**
* **NUnit Test Adapter**
* **HgSccPackage**
* **CodeLineage**

### Code coverage

Install free version of [TestDriven.Net](http://www.testdriven.net/) that includes NCover.

### Mercurial Client

Preferred client: [TortoiseHg](http://tortoisehg.bitbucket.org/).

## Checklists

### New Country Checklist

- Add Currency in ReferenceData\Currency.sql script
- Add Country in ReferenceData\Country.sql script
- Add Country Id in `CountryId` enumeration script
- Add two Resource files in CountryVariants folder with names **ResourcesXX** and **ResourceXX.yy**, where **XX** is the Id of the Country and **yy** is the culture code of the language of the country.
- Add field metadata in `CountryAddressFieldMetadata` constants class.

### New Language Checklist

- Add Language in ReferenceData\Language.sql script
