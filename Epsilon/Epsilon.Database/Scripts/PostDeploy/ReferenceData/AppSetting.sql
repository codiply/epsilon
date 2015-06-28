-- PostDeploy\ReferenceData\AppSetting.sql START

GO
-- I drop the temporary table  #TMP if it exists.
IF OBJECT_ID('tempdb..#TMP') IS NOT NULL DROP TABLE #TMP
GO

-- I copy the schema of the source table into #TMP.
SELECT * INTO #TMP
FROM [dbo].[AppSetting]
WHERE 1 = 0

INSERT INTO #TMP
([Id], [Value], [ValueType], [Description])
VALUES
-- Edit the values below to update the target table.
(N'AdminAlertSnoozePeriodInHours', N'12.0', N'Double',
 N'The AdminAlertService will wait this amount of time (in hours) until it sends a second alert for any given AdminAlert key.'),
(N'SearchAddressResultsLimit', N'30', N'Int',
 N'The number of results returned when performing and Address Search.');
GO

MERGE [dbo].[AppSetting] AS T -- Target
USING #TMP AS S -- Source
    ON T.Id = S.Id
WHEN MATCHED
    THEN UPDATE SET
	    T.[Value] = S.[Value],
		T.[ValueType] = S.[ValueType],
		T.[Description] = S.[Description]
WHEN NOT MATCHED
    THEN INSERT ([Id], [Value], [ValueType], [Description])
	VALUES (S.[Id], S.[Value], S.[ValueType], S.[Description])
WHEN NOT MATCHED BY SOURCE 
    THEN DELETE;
GO

DROP TABLE #TMP;
GO

-- PostDeploy\ReferenceData\AppSetting.sql END