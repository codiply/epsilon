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
 N'The number of results returned when performing and Address Search.'),
(N'AntiAbuse_AddAddress_MaxFrequencyPerUser', N'2/30D', N'Frequency',
 N'The maximum number of address a user can add in a certain period of time.'),
(N'AntiAbuse_AddAddress_MaxFrequencyPerIpAddress', N'2/D', N'Frequency',
 N'The maximum number of addresses that can be added by an ip address in a certain period of time.'),
(N'AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser', N'1/30D', N'Frequency',
 N'The maximum number of tenancy details submissions a user can create in a certain period of time.'),
(N'AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress', N'2/D', N'Frequency',
 N'The maximum number of tenancy details submissions that can becreated by an ip address in a certain period of time.');
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