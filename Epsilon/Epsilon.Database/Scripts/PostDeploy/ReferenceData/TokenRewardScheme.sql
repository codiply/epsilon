-- PostDeploy\ReferenceData\TokenRewardSchemeType.sql START

GO
-- I drop the temporary table  #TMP if it exists.
IF OBJECT_ID('tempdb..#TMP') IS NOT NULL DROP TABLE #TMP
GO

-- I copy the schema of the source table into #TMP.
SELECT * INTO #TMP
FROM [dbo].[TokenRewardScheme]
WHERE 1 = 0

INSERT INTO #TMP
([Id], [EffectiveFrom])
VALUES
-- !!! IMPORTANT !!! 
-- Do not edit the values for the current or past schemes. 
-- Only insert new schemes.
(N'1', cast('2015-06-01T00:00:00.000+00:00' AS DateTimeOffset));
GO

MERGE [dbo].[TokenRewardScheme] AS T -- Target
USING #TMP AS S -- Source
    ON T.Id = S.Id
WHEN MATCHED
    THEN UPDATE SET
	    T.[EffectiveFrom] = S.[EffectiveFrom]
WHEN NOT MATCHED
    THEN INSERT ([Id], [EffectiveFrom])
	VALUES (S.[Id], S.[EffectiveFrom])
WHEN NOT MATCHED BY SOURCE 
    THEN DELETE;
GO

DROP TABLE #TMP;
GO

-- PostDeploy\ReferenceData\TokenRewardScheme.sql END