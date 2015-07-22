﻿-- PostDeploy\ReferenceData\TokenReward.sql START

GO
-- I drop the temporary table  #TMP if it exists.
IF OBJECT_ID('tempdb..#TMP') IS NOT NULL DROP TABLE #TMP
GO

-- I copy the schema of the source table into #TMP.
SELECT * INTO #TMP
FROM [dbo].[TokenReward]
WHERE 1 = 0

INSERT INTO #TMP
([SchemeId], [Key], [Value])
VALUES
-- !!! IMPORTANT !!! 
-- 1. Do not edit the values for the current or past schemes. Only insert new value.
-- 2. All keys should either start with Earn or Spend.
-- 3. Keys starting with Earn should have positive value.
-- 4. Keys starting with Spend should have negative value.
-- Scheme 1
(N'1', N'EarnPerTenancyDetailsSubmission', 2.0),
(N'1', N'EarnPerVerificationCodeEntered', 1.0),
(N'1', N'EarnPerVerificationMailSent', 2.0);
GO

MERGE [dbo].[TokenReward] AS T -- Target
USING #TMP AS S -- Source
    ON T.SchemeId = S.SchemeId 
	AND T.[Key] = S.[Key]
WHEN MATCHED
    THEN UPDATE SET
	    T.[Value] = S.[Value]
WHEN NOT MATCHED
    THEN INSERT ([SchemeId], [Key], [Value])
	VALUES (S.[SchemeId], S.[Key], S.[Value])
WHEN NOT MATCHED BY SOURCE 
    THEN DELETE;
GO

DROP TABLE #TMP;
GO

-- PostDeploy\ReferenceData\TokenReward.sql END