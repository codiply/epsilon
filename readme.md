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

## Email server

You can setup the application to use your gmail account to send emails by copying `Unversioned.template.config` into `Unversioned.config` and fill in your credentials. You will also need to enable access by giving [access for less secure apps](https://www.google.com/settings/security/lesssecureapps).

## Checklists

### New Country Checklist

- Add Currency in ReferenceData\Currency.sql script
- Add Country in ReferenceData\Country.sql script
- Add Country Id in `CountryId` enumeration script
- Add two Resource files in CountryVariants folder with names **ResourcesXX** and **ResourceXX.yy**, where **XX** is the Id of the Country and **yy** is the culture code of the language of the country.
- Add field metadata in `CountryAddressFieldMetadata` constants class.

### New Language Checklist

- Add Language in ReferenceData\Language.sql script

### Security Checklist

- Run security scanner [ASafaWeb](https://asafaweb.com/).

## References

- [HTTPS in ASP.NET MVC](http://tech.trailmax.info/2014/02/implemnting-https-everywhere-in-asp-net-mvc-application/)
