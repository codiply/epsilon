-- PostDeploy\ReferenceData\Countries.sql START

-- Country codes can be found here:
-- https://en.wikipedia.org/wiki/ISO_3166-1_alpha-2

GO

INSERT INTO [dbo].[Country]
([Id], [EnglishName], [LocalizedName], [CurrencyId], [IsAvailable])
VALUES
(N'GB', N'United Kingdom', N'United Kingdom', N'GBP', 1),
(N'GR', N'Greece', N'Ελλάδα', N'EUR', 1),
(N'IE', N'Ireland', N'Ireland', N'EUR', 0),
(N'US', N'United States of America', N'United States of America', N'USD', 0);

GO

-- PostDeploy\ReferenceData\Countries.sql END