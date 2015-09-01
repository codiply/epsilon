-- !!! IMPORTANT!
-- Do not add this script to PostDeployment script! 
-- Just use this directly from within SQL Management studio on your development database.
-- This is a script to use in development environment with settings for a demo over the WiFi network. 
-- For example, use of Geoip information is disabled as internal IP addresses are used over the WiFi.

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
(N'AntiAbuse_AddAddress_MaxFrequencyPerIpAddress', N'10/D'),
(N'AntiAbuse_AddAddress_MaxFrequencyPerUser', N'10/D'),
(N'AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerIpAddress', N'10/H'),
(N'AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerUser', N'5/H'),
(N'AntiAbuse_CreateTenancyDetailsSubmission_GlobalMaxFrequency', N'10/D'),
(N'AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress', N'10/D'),
(N'AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser', N'10/30D'),
(N'AntiAbuse_PickOutgoingVerification_GlobalMaxFrequency', N'10/D'),
(N'AntiAbuse_PickOutgoingVerification_MaxFrequencyPerIpAddress', N'8/3D'),
(N'AntiAbuse_PickOutgoingVerification_MaxOutstandingFrequencyPerUser', N'8/3D'),
(N'AntiAbuse_PickOutgoingVerification_MaxOutstandingFrequencyPerUserForNewUser', N'4/3D'),
(N'AntiAbuse_Register_MaxFrequencyPerIpAddress', N'10/7D'),
(N'GlobalSwitch_DisableUseOfGeoipInformation', N'True'),
(N'OutgoingVerification_MyOutgoingVerificationsSummary_ItemsLimit', N'5'),
(N'OutgoingVerification_RewardSendersIfNoneUsed_AfterPeriodInDays', N'7'),
(N'PropertInfoAccess_MyExploredPropertiesSummary_ItemsLimit', N'5'),
(N'TenancyDetailsSubmission_Create_MaxFrequencyPerAddress', '10/30D'),
(N'TenancyDetailsSubmission_MySubmissionsSummary_ItemsLimit', N'5');
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