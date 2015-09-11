-- PostDeploy\ReferenceData\Language.sql START

-- Culture codes can be found here:
-- http://timtrott.co.uk/culture-codes/

GO
-- I drop the temporary table  #TMP if it exists.
IF OBJECT_ID('tempdb..#TMP') IS NOT NULL DROP TABLE #TMP
GO

-- I copy the schema of the source table into #TMP.
SELECT * INTO #TMP
FROM [dbo].[Language]
WHERE 1 = 0

INSERT INTO #TMP
([Id], [EnglishName], [LocalName], [CultureCode], [IsAvailable])
VALUES
-- Edit the values below to update the target table.
-- !!! Id's should be lowercase !!!
(N'en', N'English', N'English', N'en-GB', 1),
(N'el', N'Greek', N'Ελληνικά', N'el-GR', 0),
(N'us', N'English (US)', N'English (US)', N'en-US', 0);
GO

MERGE [dbo].[Language] AS T -- Target
USING #TMP AS S -- Source
    ON T.Id = S.Id
WHEN MATCHED
    THEN UPDATE SET
        T.[EnglishName] = S.[EnglishName],
        T.[LocalName] = S.[LocalName],
        T.[CultureCode] = S.[CultureCode],
        T.[IsAvailable] = S.[IsAvailable]
WHEN NOT MATCHED
    THEN INSERT ([Id], [EnglishName], [LocalName], [CultureCode], [IsAvailable])
    VALUES (S.[Id], S.[EnglishName], S.[LocalName], S.[CultureCode], S.[IsAvailable])
WHEN NOT MATCHED BY SOURCE 
    THEN DELETE;
GO

DROP TABLE #TMP;
GO

-- PostDeploy\ReferenceData\Language.sql END