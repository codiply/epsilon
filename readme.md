## Development environment setup

### Visual studio

Install Visual Studio 2015 Community Edition. 

Choose **Custom** as the type of Installation and then install either all features, or at least the following:

- Microsoft SQL Server Data Tools
- Microsoft Web Developer Tools

From **Tools > Extension and Updates...** install

* **Typescript 1.5 for Visual Studio 2015** (if not installed)
* **NUnit Test Adapter**
* **Productivity Power Tools 2015**
* **HgSccPackage**
* **CodeLineage**

### SQL Server

Install SQL Server 2014 Express with Advanced Services 64 Bit. In the Instance Configuration step choose **Default Instance**.

### Code coverage

Install free version of [TestDriven.Net](http://www.testdriven.net/) that includes NCover.

### Mercurial Client

Preferred client: [TortoiseHg](http://tortoisehg.bitbucket.org/).

### NUnit Gui Runner

Download and install from [here](http://www.nunit.org/index.php?p=download).
The NUnit Gui Runner groups tests in a better way than running them from within Visual Studio.

## Email server

You can setup the application to use your gmail account to send emails by copying `Unversioned.template.config` into `Unversioned.config` and fill in your credentials. You will also need to enable access by giving [access for less secure apps](https://www.google.com/settings/security/lesssecureapps).

## Google Api Key

You will need to use a Google Api Key in order to access the [Google Geocoding Api](https://developers.google.com/maps/documentation/geocoding/intro) (see link on how to setup one). The key should be entered in the `Unversioned.config`. 

## Running the Integration Tests

To run the integration tests publish first the Database project into a new local database named **EpsilonTest**.  

## Tips and Tricks

- DbAppSettings are cached so you will need to restart the website for any changes to take effect.
- You can simulate a different IP address by setting `Epsilon.IpAddressOverride` in the `Web.config`
- There are many anti-abuse and other checks in place that limit the number of times you can perform actions. You can change these settings in the database or use `Scripts/DevScripts/DevAppSettings.sql` by changing the values and running it on your database. 
- If you serve and test the application in a WLAN, do not set an IpAddressOverride but set `GlobalSwitch_DisableUseOfGeoipInformation` in  database AppSettings.

## Checklists

### New DbAppSetting Checklist

- Add setting key to `Constants/Enums/DbAppSettingsKey`
- Add setting in `Scripts/PostDeploy/ReferenceData/AppSetting.sql` script
- Add labels for new key in `Scripts/PostDeploy/ReferenceData/AppSettingLabel.sql` script
- Add setting in `Scripts/DevScripts/DevAppSettings.sql` script
- Update your test database and run your tests (as always).

### New Country Checklist

- Add Currency in `ReferenceData/Currency.sql` script
- Add Country in `ReferenceData/Country.sql` script
- Add Country Id in `CountryId` enumeration script
- Add two Resource files in CountryVariants folder with names **ResourcesXX** and **ResourceXX.yy**, where **XX** is the Id of the Country and **yy** is the culture code of the language of the country.
- Add field metadata in `CountryAddressFieldMetadata` constants class.
- Add case for new country in AddressCleansingHelper.CleansePostcode method.
- Add Country-specific integration tests in GeocodeServiceTest
- Add case for new country in `Views/OutgoingVerification/_VerificationRecipient.cshtml`.
- Add case for new country in `Views/OutgoingVerification/_VerificationMessage.cshtml`.

### New Language Checklist

- Add Language in ReferenceData\Language.sql script
- Check that moment.js localization includes the new language and works (see token transactions page for example).

### Security Checklist

- Run security scanner [ASafaWeb](https://asafaweb.com/).

## References

- [HTTPS in ASP.NET MVC](http://tech.trailmax.info/2014/02/implemnting-https-everywhere-in-asp-net-mvc-application/)
- [ASP.NET Identity and IoC Container Registration](http://tech.trailmax.info/2014/09/aspnet-identity-and-ioc-container-registration/)
