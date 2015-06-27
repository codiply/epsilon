-- PostDeploy\ReferenceData\Countries.sql START

-- Country codes can be found here:
-- https://en.wikipedia.org/wiki/ISO_3166-1_alpha-2

-- NOTE 1: The id should be UPERCASE.
-- NOTE 2: The Id for any available country here should also be added to the CountryId enum.

GO
-- I drop the temporary table  #TMP if it exists.
IF OBJECT_ID('tempdb..#TMP') IS NOT NULL DROP TABLE #TMP
GO

-- I copy the schema of the source table into #TMP.
SELECT * INTO #TMP
FROM [dbo].[Country]
WHERE 1 = 0

INSERT INTO #TMP
([Id], [EnglishName], [LocalName], [CurrencyId], [IsAvailable])
VALUES
-- Edit the values below to update the target table.
(N'GB', N'United Kingdom', N'United Kingdom', N'GBP', 1),
(N'GR', N'Greece', N'Ελλάδα', N'EUR', 1),
(N'IE', N'Ireland', N'Ireland', N'EUR', 0),
(N'US', N'United States of America', N'United States of America', N'USD', 0);
GO

MERGE [dbo].[Country] AS T -- Target
USING #TMP AS S -- Source
    ON T.Id = S.Id
WHEN MATCHED
    THEN UPDATE SET
	    T.[EnglishName] = S.[EnglishName],
		T.[LocalName] = S.[LocalName],
		T.[CurrencyId] = S.[CurrencyId],
		T.[IsAvailable] = S.[IsAvailable]
WHEN NOT MATCHED
    THEN INSERT ([Id], [EnglishName], [LocalName], [CurrencyId], [IsAvailable])
	VALUES (S.[Id], S.[EnglishName], S.[LocalName], S.[CurrencyId], S.[IsAvailable])
WHEN NOT MATCHED BY SOURCE 
    THEN DELETE;
GO

DROP TABLE #TMP;
GO

-- PostDeploy\ReferenceData\Countries.sql END