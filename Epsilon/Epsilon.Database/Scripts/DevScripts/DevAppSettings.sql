-- !!! IMPORTANT!
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
(N'Address_SearchAddressResultsLimit', N'50'),
(N'Address_SearchPropertyResultsLimit', N'50'),
(N'AdminAlertSnoozePeriodInHours', N'12.0'),
(N'AntiAbuse_AddAddress_DisableGeocodeFailureIpAddressFrequencyCheck', 'False'),
(N'AntiAbuse_AddAddress_DisableGeocodeFailureUserFrequencyCheck', 'False'),
(N'AntiAbuse_AddAddress_DisableGeoipCheck', N'False'),
(N'AntiAbuse_AddAddress_DisableGlobalFrequencyCheck', 'False'),
(N'AntiAbuse_AddAddress_DisableIpAddressFrequencyCheck', N'False'),
(N'AntiAbuse_AddAddress_DisableUserFrequencyCheck', N'False'),
(N'AntiAbuse_AddAddress_GlobalMaxFrequency', N'10/D'),
(N'AntiAbuse_AddAddress_MaxFrequencyPerIpAddress', N'10/D'),
(N'AntiAbuse_AddAddress_MaxFrequencyPerUser', N'10/30D'),
(N'AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerIpAddress', N'8/H'),
(N'AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerUser', N'4/2H'),
(N'AntiAbuse_CreateTenancyDetailsSubmission_DisableGeoipCheck', N'False'),
(N'AntiAbuse_CreateTenancyDetailsSubmission_DisableGlobalFrequencyCheck', N'False'),
(N'AntiAbuse_CreateTenancyDetailsSubmission_DisableIpAddressFrequencyCheck', N'False'),
(N'AntiAbuse_CreateTenancyDetailsSubmission_DisableUserFrequencyCheck', N'False'),
(N'AntiAbuse_CreateTenancyDetailsSubmission_GlobalMaxFrequency', N'10/D'),
(N'AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress', N'10/D'),
(N'AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser', N'10/30D'),
(N'AntiAbuse_PickOutgoingVerification_DisableGeoipCheck', N'False'),
(N'AntiAbuse_PickOutgoingVerification_DisableGlobalFrequencyCheck', N'False'),
(N'AntiAbuse_PickOutgoingVerification_DisableIpAddressFrequencyCheck', N'False'),
(N'AntiAbuse_PickOutgoingVerification_DisableMaxOutstandingFrequencyPerUserCheck', N'False'),
(N'AntiAbuse_PickOutgoingVerification_GlobalMaxFrequency', N'10/D'),
(N'AntiAbuse_PickOutgoingVerification_MaxFrequencyPerIpAddress', N'8/3D'),
(N'AntiAbuse_PickOutgoingVerification_MaxOutstandingFrequencyPerUser', N'8/60D'),
(N'AntiAbuse_PickOutgoingVerification_MaxOutstandingFrequencyPerUserForNewUser', N'4/60D'),
(N'AntiAbuse_Register_DisableGlobalFrequencyCheck', N'False'),
(N'AntiAbuse_Register_DisableIpAddressFrequencyCheck', N'False'),
(N'AntiAbuse_Register_GlobalMaxFrequency', N'300/D'),
(N'AntiAbuse_Register_MaxFrequencyPerIpAddress', N'10/7D'),
(N'EnableResponseTiming', N'True'),
(N'GeocodeService_OverQueryLimitDelayBetweenRetriesInSeconds', N'1.0'),
(N'GeocodeService_OverQueryLimitMaxRetries', N'3'),
(N'GeoipInfo_ExpiryPeriodInDays', N'30'),
(N'GeoipRotatingClient_MaxRotations', N'2'),
(N'GeoipRotatingClient_ProviderRotation', N'Telize,Freegeoip'),
(N'GlobalSwitch_DisableAddAddress', N'False'),
(N'GlobalSwitch_DisableCreatePropertyInfoAccess', N'False'),
(N'GlobalSwitch_DisableCreateTenancyDetailsSubmission', N'False'),
(N'GlobalSwitch_DisablePickOutgoingVerification', N'False'),
(N'GlobalSwitch_DisableRegister', N'False'),
(N'GlobalSwitch_DisableUseOfGeoipInformation', N'False'),
(N'OutgoingVerification_Instructions_ExpiryPeriodInDays', N'7.0'),
(N'OutgoingVerification_MyOutgoingVerificationsSummary_CachingPeriodInMinutes', N'15.0'),
(N'OutgoingVerification_MyOutgoingVerificationsSummary_ItemsLimit', N'2'),
(N'OutgoingVerification_RewardSendersIfNoneUsed_AfterPeriodInDays', N'30.0'),
(N'OutgoingVerification_VerificationsPerTenancyDetailsSubmission', N'2'),
(N'PropertInfoAccess_ExpiryPeriodInDays', N'30'),
(N'PropertInfoAccess_MyExploredPropertiesSummary_CachingPeriodInMinutes', N'15.0'),
(N'PropertInfoAccess_MyExploredPropertiesSummary_ItemsLimit', N'2'),
(N'TenancyDetailsSubmission_Create_MaxFrequencyPerAddress', '10/30D'),
(N'TenancyDetailsSubmission_Create_DisableFrequencyPerAddressCheck', N'False'),
(N'TenancyDetailsSubmission_MySubmissionsSummary_CachingPeriodInMinutes', N'15.0'),
(N'TenancyDetailsSubmission_MySubmissionsSummary_ItemsLimit', N'2'),
(N'Token_MyTokenTransactions_PageSize', N'30');
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