-- PostDeploy\ReferenceData\AppSettingLabel.sql START

GO
-- I drop the temporary table  #TMP if it exists.
IF OBJECT_ID('tempdb..#TMP') IS NOT NULL DROP TABLE #TMP
GO

-- I copy the schema of the source table into #TMP.
SELECT * INTO #TMP
FROM [dbo].[AppSettingLabel]
WHERE 1 = 0

INSERT INTO #TMP
([Label], [AppSettingId])
VALUES
-- Edit the values below to update the target table.
(N'switch', N'EnableResponseTiming'); 
GO

MERGE [dbo].[AppSettingLabel] AS T -- Target
USING #TMP AS S -- Source
    ON T.Label = S.Label AND T.AppSettingId = S.AppSettingId
WHEN NOT MATCHED
    THEN INSERT ([Label], [AppSettingId])
	VALUES (S.[Label], S.[AppSettingId])
WHEN NOT MATCHED BY SOURCE 
    THEN DELETE;
GO

DROP TABLE #TMP;
GO

-- PostDeploy\ReferenceData\AppSettingLabel.sql END