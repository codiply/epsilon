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
(N'Address_SearchAddressResultsLimit', N'50', N'Integer',
     N'The number of results returned when performing and Address Search.'),
(N'Address_SearchPropertyResultsLimit', N'50', N'Integer',
     N'The number of results returned when performing and Property Search.'),
(N'AdminAlertSnoozePeriodInHours', N'12.0', N'Double',
     N'The AdminAlertService will wait this amount of time (in hours) until it sends a second alert for any given AdminAlert key.'),
(N'AntiAbuse_AddAddress_DisableGeocodeFailureIpAddressFrequencyCheck', 'False', N'Boolean',
     N'Disables the anti-abuse IP Address frequency check for geocode failures.'),
(N'AntiAbuse_AddAddress_DisableGeocodeFailureUserFrequencyCheck', 'False', N'Boolean',
     N'Disables the anti-abuse uer frequency check for geocode failures.'),
(N'AntiAbuse_AddAddress_DisableGlobalFrequencyCheck', 'False', N'Boolean',
     N'Disables the anti-abuse global frequency check for add a new address.'),
(N'AntiAbuse_AddAddress_DisableIpAddressFrequencyCheck', N'False', N'Boolean',
     N'Disables the anti-abuse IP Address frequency check when adding a new address.'),
(N'AntiAbuse_AddAddress_DisableUserFrequencyCheck', N'False', N'Boolean',
     N'Disables the anti-abuse user frequency check when adding a new address.'),
-- The geocoding api gives us 2500 queries per day and we do one query for the postcode and one for the address.
-- 1000 addresses per day means 2000 queries per day plus some spare queries for failures.
-- https://developers.google.com/maps/documentation/geocoding/intro
(N'AntiAbuse_AddAddress_GlobalMaxFrequency', N'1000/D', N'Frequency',
     N'The maximum number of addresses that can be added in a given period of time.'),
(N'AntiAbuse_AddAddress_MaxFrequencyPerIpAddress', N'2/D', N'Frequency',
     N'The maximum number of addresses that can be added by an ip address in a certain period of time.'),
(N'AntiAbuse_AddAddress_MaxFrequencyPerUser', N'2/30D', N'Frequency',
     N'The maximum number of address a user can add in a certain period of time.'),
(N'AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerIpAddress', N'8/H', N'Frequency',
     N'The maximum number of times a user can fail geocoding when adding an address by an ip address in a certain period of time.'),
(N'AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerUser', N'4/2H', N'Frequency',
     N'The maximum number of times a user can fail geocoding when adding an address in a certain period of time.'),
(N'AntiAbuse_CreateTenancyDetailsSubmission_DisableGlobalFrequencyCheck', N'False', N'Boolean',
     N'Disables the anti-abuse global frequency check for creating a new tenancy details submission.'),
(N'AntiAbuse_CreateTenancyDetailsSubmission_DisableIpAddressFrequencyCheck', N'False', N'Boolean',
     N'Disables the anti-abuse IP Address frequency check when creating a new tenancy details submission.'),
(N'AntiAbuse_CreateTenancyDetailsSubmission_DisableUserFrequencyCheck', N'False', N'Boolean',
     N'Disables the anti-abuse user frequency check when creating a new tenancy details submission.'),
(N'AntiAbuse_CreateTenancyDetailsSubmission_GlobalMaxFrequency', N'10000/D', N'Frequency',
     N'The maximum number tenancy details submissions that can be created in a given period of time.'),
(N'AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress', N'2/D', N'Frequency',
     N'The maximum number of tenancy details submissions that can becreated by an ip address in a certain period of time.'),
(N'AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser', N'1/30D', N'Frequency',
     N'The maximum number of tenancy details submissions a user can create in a certain period of time.'),
(N'AntiAbuse_PickOutgoingVerification_DisableGlobalFrequencyCheck', N'False', N'Boolean',
     N'Disables the anti-abuse global frequency check for picking a new outgoing verification.'),
(N'AntiAbuse_PickOutgoingVerification_DisableIpAddressFrequencyCheck', N'False', N'Boolean',
     N'Disables the anti-abuse IP Address frequency check when creating a outgoing verification.'),
(N'AntiAbuse_PickOutgoingVerification_DisableMaxOutstandingFrequencyPerUserCheck', N'False', N'Boolean',
     N'Disables the anti-abuse check for the number of outstanding outgoing verificiations a user has.'),
(N'AntiAbuse_PickOutgoingVerification_GlobalMaxFrequency', N'10000/D', N'Frequency',
     N'The maximum number of outgoing verifications that can be created in a given period of time.'),
(N'AntiAbuse_PickOutgoingVerification_MaxFrequencyPerIpAddress', N'8/3D', N'Frequency',
     N'The maximum number of outgoing verifications that can be created by an ip address in a certain period of time.'),
(N'AntiAbuse_PickOutgoingVerification_MaxOutstandingFrequencyPerUser', N'8/60D', N'Frequency',
     N'The number maximum outstanding outgoing verifications for a user without completed outgoing verifications in a given period of time.'), 
(N'AntiAbuse_PickOutgoingVerification_MaxOutstandingFrequencyPerUserForNewUser', N'4/60D', N'Frequency',
     N'The number maximum outstanding outgoing verifications for a user without completed outgoing verifications in a given period of time.'), 
(N'AntiAbuse_Register_DisableGlobalFrequencyCheck', N'False', N'Boolean',
     N'Disables the anti-abuse global frequency check when a user registers.'),
(N'AntiAbuse_Register_DisableIpAddressFrequencyCheck', N'False', N'Boolean',
     N'Disables the anti-abuse IP Address frequency check when a user registers.'),
(N'AntiAbuse_Register_GlobalMaxFrequency', N'300/D', N'Frequency',
     N'The maximum number of users that can register in a given period of time.'),
(N'AntiAbuse_Register_MaxFrequencyPerIpAddress', N'3/7D', N'Frequency',
     N'The maximum number of users that can register from a single IP address in a given period of time.'),
(N'EnableResponseTiming', N'True', N'Boolean',
     N'Enables the logging of the response time for each response.'),
(N'GeocodeService_OverQueryLimitDelayBetweenRetriesInSeconds', N'1.0', N'Double',
     N'Dealy between retries when Google Geocode API responds with OverQueryLimit.'),
(N'GeocodeService_OverQueryLimitMaxRetries', N'3', N'Integer',
     N'Maximum number of retries when Google Geocode API responds with OverQueryLimit. Set to zero for no retrying.'),
(N'GlobalSwitch_DisableAddAddress', N'False', N'Boolean',
     N'Disables completely adding a new address.'),
(N'GlobalSwitch_DisableCreateTenancyDetailsSubmission', N'False', N'Boolean',
     N'Disables completely creating a new tenancy details submission.'),
(N'GlobalSwitch_DisablePickOutgoingVerification', N'False', N'Boolean',
     N'Disables completely creating a new outgoing verification.'),
(N'GlobalSwitch_DisableRegister', N'False', N'Boolean',
     N'Disables completely the registration of new users.'),
(N'OutgoingVerification_MyOutgoingVerificationsSummary_CachingPeriodInMinutes', N'15.0', N'Double',
     N'Caching period in minutes for MyOutgoingVerificationsSummary data.'),
(N'OutgoingVerification_MyOutgoingVerificationsSummary_ItemsLimit', N'10', N'Integer',
     N'The maximum number of outgoing verifications that will apear in the summary on the front page.'),
(N'OutgoingVerification_VerificationsPerTenancyDetailsSubmission', N'2', N'Integer',
     N'The number of verifications to be assigned per tenancy details submission.'),
(N'PropertInfoAccess_MyExploredPropertiesSummary_CachingPeriodInMinutes', N'15.0', N'Double',
     N'Caching period in minutes for MyExploredPropertiesSummary data.'),
(N'PropertInfoAccess_MyExploredPropertiesSummary_ItemsLimit', N'10', N'Integer',
     N'The maximum number of explored properties that will apear in the summary on the front page.'),
(N'TenancyDetailsSubmission_Create_MaxFrequencyPerAddress', N'1/30D', N'Frequency',
     N'The maximum number of TenancyDetailsSubmission''s that can be created per address in a given period.'),
(N'TenancyDetailsSubmission_Create_DisableFrequencyPerAddressCheck', N'False', N'Boolean',
     N'Disables the FrequencyPerAddress check when creating a new TenancyDetailsSubmission'),
(N'TenancyDetailsSubmission_MySubmissionsSummary_CachingPeriodInMinutes', N'15.0', N'Double',
     N'Caching period in minutes for MySubmissionSummary data.'),
(N'TenancyDetailsSubmission_MySubmissionsSummary_ItemsLimit', N'10', N'Integer',
     N'The maximum number of submissions that will apear in the summary on the front page.'),
-- This needs to be big enough to fill one screen with the first page and a scroll bar appears.
(N'Token_MyTokenTransactions_PageSize', N'30', N'Integer',
     N'The number of items fetched as a page from the server for MyTokenTrasactions screen.');
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