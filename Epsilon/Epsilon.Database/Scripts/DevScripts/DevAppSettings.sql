﻿-- !!! IMPORTANT!
-- Do not add this script to PostDeployment script! 
-- Just use this directly from within SQL Management studio on your development database.
-- This is a script to use in development environment to use settings that are more
-- friendly to testing. For example to allow to register several address without hitting
-- an anti-abuse limit.

GO
-- I drop the temporary table  #TMP if it exists.
IF OBJECT_ID('tempdb..#TMP') IS NOT NULL DROP TABLE #TMP
GO

SELECT [Id], [Value] INTO #TMP
FROM [dbo].[AppSetting]
WHERE 1 = 0


INSERT INTO #TMP
([Id], [Value])
VALUES
-- Edit the values below to update the target table.
(N'AdminAlertSnoozePeriodInHours', N'12.0'),
(N'SearchAddressResultsLimit', N'30'),
(N'AntiAbuse_AddAddress_MaxFrequencyPerUser', N'10/30D'),
(N'AntiAbuse_AddAddress_MaxFrequencyPerIpAddress', N'10/D'),
(N'AntiAbuse_AddAddress_DisableIpAddressFrequencyCheck', N'False'),
(N'AntiAbuse_AddAddress_DisableUserFrequencyCheck', N'False'),
(N'AntiAbuse_CreateTenancyDetailsSubmission_DisableIpAddressFrequencyCheck', N'False'),
(N'AntiAbuse_CreateTenancyDetailsSubmission_DisableUserFrequencyCheck', N'False'),
(N'AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress', N'10/D'),
(N'AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser', N'10/30D'),
(N'AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerUser', N'4/2H'),
(N'AntiAbuse_AddAddress_DisableGeocodeFailureUserFrequencyCheck', 'False'),
(N'AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerIpAddress', N'8/H'),
(N'AntiAbuse_AddAddress_DisableGeocodeFailureIpAddressFrequencyCheck', 'False'),
(N'AntiAbuse_Register_GlobalMaxFrequency', '300/D'),
(N'AntiAbuse_Register_DisableGlobalFrequencyCheck', 'False'),
(N'AntiAbuse_Register_MaxFrequencyPerIpAddress', '10/7D'),
(N'AntiAbuse_Register_DisableIpAddressFrequencyCheck', 'False'),
(N'GeocodeService_OverQueryLimitMaxRetries', N'3'),
(N'GeocodeService_OverQueryLimitDelayBetweenRetriesInSeconds', N'1.0'),
(N'TenancyDetailsSubmission_Create_DisableFrequencyPerAddressCheck', 'False'),
(N'TenancyDetailsSubmission_Create_MaxFrequencyPerAddress', '10/30D'),
(N'TenancyDetailsSubmission_MySubmissionsSummary_ItemsLimit', N'2'),
(N'EnableResponseTiming', N'True'); 
GO

MERGE [dbo].[AppSetting] AS T -- Target
USING #TMP AS S -- Source
    ON T.Id = S.Id
WHEN MATCHED
    THEN UPDATE SET
	    T.[Value] = S.[Value];
GO

DROP TABLE #TMP;
GO