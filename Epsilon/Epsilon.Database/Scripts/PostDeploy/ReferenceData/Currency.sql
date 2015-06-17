-- PostDeploy\ReferenceData\Currency.sql START

-- Currency codes and symbols can be found here:
-- http://www.xe.com/symbols.php

GO

INSERT INTO [dbo].[Currency]
([Id], [EnglishName], [Symbol])
VALUES
(N'EUR', N'Euro', N'€'),
(N'GBP', N'United Kingdom Pound', N'£'),
(N'USD', N'United States Dollar', N'$');

GO

-- PostDeploy\ReferenceData\Currency.sql END