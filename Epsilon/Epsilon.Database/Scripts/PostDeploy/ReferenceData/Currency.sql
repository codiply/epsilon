-- PostDeploy\ReferenceData\Currency.sql START

-- Currency codes and symbols can be found here:
-- http://www.xe.com/symbols.php

GO
-- I drop the temporary table  #TMP if it exists.
IF OBJECT_ID('tempdb..#TMP') IS NOT NULL DROP TABLE #TMP
GO

-- I copy the schema of the source table into #TMP.
SELECT * INTO #TMP
FROM [dbo].[Currency]
WHERE 1 = 0

INSERT INTO #TMP
([Id], [EnglishName], [Symbol])
VALUES
-- Edit the values below to update the target table.
(N'EUR', N'Euro', N'€'),
(N'GBP', N'United Kingdom Pound', N'£'),
(N'USD', N'United States Dollar', N'$');
GO

MERGE [dbo].[Currency] AS T -- Target
USING #TMP AS S -- Source
    ON T.Id = S.Id
WHEN MATCHED
    THEN UPDATE SET
	    T.[EnglishName] = S.[EnglishName],
		T.[Symbol] = S.[Symbol]
WHEN NOT MATCHED
    THEN INSERT ([Id], [EnglishName], [Symbol])
	VALUES (S.[Id], S.[EnglishName], S.[Symbol])
WHEN NOT MATCHED BY SOURCE 
    THEN DELETE;
GO

DROP TABLE #TMP;
GO

-- PostDeploy\ReferenceData\Currency.sql END