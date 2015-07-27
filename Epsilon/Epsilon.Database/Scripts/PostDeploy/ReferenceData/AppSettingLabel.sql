﻿-- PostDeploy\ReferenceData\AppSettingLabel.sql START

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
-- anti-abuse
(N'anti-abuse', N'AntiAbuse_AddAddress_DisableGeocodeFailureIpAddressFrequencyCheck'),
(N'anti-abuse', N'AntiAbuse_AddAddress_DisableGeocodeFailureUserFrequencyCheck'),
(N'anti-abuse', N'AntiAbuse_AddAddress_DisableGlobalFrequencyCheck'),
(N'anti-abuse', N'AntiAbuse_AddAddress_DisableIpAddressFrequencyCheck'),
(N'anti-abuse', N'AntiAbuse_AddAddress_DisableUserFrequencyCheck'),
(N'anti-abuse', N'AntiAbuse_AddAddress_GlobalMaxFrequency'),
(N'anti-abuse', N'AntiAbuse_AddAddress_MaxFrequencyPerIpAddress'),
(N'anti-abuse', N'AntiAbuse_AddAddress_MaxFrequencyPerUser'),
(N'anti-abuse', N'AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerIpAddress'),
(N'anti-abuse', N'AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerUser'),
(N'anti-abuse', N'AntiAbuse_CreateTenancyDetailsSubmission_DisableGlobalFrequencyCheck'),
(N'anti-abuse', N'AntiAbuse_CreateTenancyDetailsSubmission_DisableIpAddressFrequencyCheck'),
(N'anti-abuse', N'AntiAbuse_CreateTenancyDetailsSubmission_DisableUserFrequencyCheck'),
(N'anti-abuse', N'AntiAbuse_CreateTenancyDetailsSubmission_GlobalMaxFrequency'),
(N'anti-abuse', N'AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress'),
(N'anti-abuse', N'AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser'),
(N'anti-abuse', N'AntiAbuse_PickOutgoingVerification_DisableGlobalFrequencyCheck'),
(N'anti-abuse', N'AntiAbuse_PickOutgoingVerification_GlobalMaxFrequency'),
(N'anti-abuse', N'AntiAbuse_Register_DisableGlobalFrequencyCheck'),
(N'anti-abuse', N'AntiAbuse_Register_DisableIpAddressFrequencyCheck'),
(N'anti-abuse', N'AntiAbuse_Register_GlobalMaxFrequency'),
(N'anti-abuse', N'AntiAbuse_Register_MaxFrequencyPerIpAddress'),
-- anti-abuse-add-address
(N'anti-abuse-add-address', N'AntiAbuse_AddAddress_DisableGeocodeFailureIpAddressFrequencyCheck'),
(N'anti-abuse-add-address', N'AntiAbuse_AddAddress_DisableGeocodeFailureUserFrequencyCheck'),
(N'anti-abuse-add-address', N'AntiAbuse_AddAddress_DisableGlobalFrequencyCheck'),
(N'anti-abuse-add-address', N'AntiAbuse_AddAddress_DisableIpAddressFrequencyCheck'),
(N'anti-abuse-add-address', N'AntiAbuse_AddAddress_DisableUserFrequencyCheck'),
(N'anti-abuse-add-address', N'AntiAbuse_AddAddress_GlobalMaxFrequency'),
(N'anti-abuse-add-address', N'AntiAbuse_AddAddress_MaxFrequencyPerIpAddress'),
(N'anti-abuse-add-address', N'AntiAbuse_AddAddress_MaxFrequencyPerUser'),
(N'anti-abuse-add-address', N'AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerIpAddress'),
(N'anti-abuse-add-address', N'AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerUser'),
-- anti-abuse-create-tenancy-details-submission
(N'anti-abuse-create-tenancy-details-submission', N'AntiAbuse_CreateTenancyDetailsSubmission_DisableGlobalFrequencyCheck'),
(N'anti-abuse-create-tenancy-details-submission', N'AntiAbuse_CreateTenancyDetailsSubmission_DisableIpAddressFrequencyCheck'),
(N'anti-abuse-create-tenancy-details-submission', N'AntiAbuse_CreateTenancyDetailsSubmission_DisableUserFrequencyCheck'),
(N'anti-abuse-create-tenancy-details-submission', N'AntiAbuse_CreateTenancyDetailsSubmission_GlobalMaxFrequency'),
(N'anti-abuse-create-tenancy-details-submission', N'AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress'),
(N'anti-abuse-create-tenancy-details-submission', N'AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser'),
-- anti-abuse-ip-address
(N'anti-abuse-ip-address', N'AntiAbuse_AddAddress_DisableGeocodeFailureIpAddressFrequencyCheck'),
(N'anti-abuse-ip-address', N'AntiAbuse_AddAddress_DisableIpAddressFrequencyCheck'),
(N'anti-abuse-ip-address', N'AntiAbuse_AddAddress_MaxFrequencyPerIpAddress'),
(N'anti-abuse-ip-address', N'AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerIpAddress'),
(N'anti-abuse-ip-address', N'AntiAbuse_CreateTenancyDetailsSubmission_DisableIpAddressFrequencyCheck'),
(N'anti-abuse-ip-address', N'AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress'),
(N'anti-abuse-ip-address', N'AntiAbuse_Register_DisableIpAddressFrequencyCheck'),
(N'anti-abuse-ip-address', N'AntiAbuse_Register_MaxFrequencyPerIpAddress'),
-- anti-abuse-max-frequency
(N'anti-abuse-max-frequency', N'AntiAbuse_AddAddress_GlobalMaxFrequency'),
(N'anti-abuse-max-frequency', N'AntiAbuse_AddAddress_MaxFrequencyPerIpAddress'),
(N'anti-abuse-max-frequency', N'AntiAbuse_AddAddress_MaxFrequencyPerUser'),
(N'anti-abuse-max-frequency', N'AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerIpAddress'),
(N'anti-abuse-max-frequency', N'AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerUser'),
(N'anti-abuse-max-frequency', N'AntiAbuse_CreateTenancyDetailsSubmission_GlobalMaxFrequency'),
(N'anti-abuse-max-frequency', N'AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress'),
(N'anti-abuse-max-frequency', N'AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser'),
(N'anti-abuse-max-frequency', N'AntiAbuse_PickOutgoingVerification_GlobalMaxFrequency'),
(N'anti-abuse-max-frequency', N'AntiAbuse_Register_GlobalMaxFrequency'),
(N'anti-abuse-max-frequency', N'AntiAbuse_Register_MaxFrequencyPerIpAddress'),
-- anti-abuse-pick-outgoing-verification
(N'anti-abuse-pick-outgoing-verification', N'AntiAbuse_PickOutgoingVerification_DisableGlobalFrequencyCheck'),
(N'anti-abuse-pick-outgoing-verification', N'AntiAbuse_PickOutgoingVerification_GlobalMaxFrequency'),
-- anti-abuse-register
(N'anti-abuse-register', N'AntiAbuse_Register_DisableGlobalFrequencyCheck'),
(N'anti-abuse-register', N'AntiAbuse_Register_DisableIpAddressFrequencyCheck'),
(N'anti-abuse-register', N'AntiAbuse_Register_GlobalMaxFrequency'),
(N'anti-abuse-register', N'AntiAbuse_Register_MaxFrequencyPerIpAddress'),
-- anti-abuse-switch
(N'anti-abuse-switch', N'AntiAbuse_AddAddress_DisableGeocodeFailureIpAddressFrequencyCheck'),
(N'anti-abuse-switch', N'AntiAbuse_AddAddress_DisableGeocodeFailureUserFrequencyCheck'),
(N'anti-abuse-switch', N'AntiAbuse_AddAddress_DisableGlobalFrequencyCheck'),
(N'anti-abuse-switch', N'AntiAbuse_AddAddress_DisableIpAddressFrequencyCheck'),
(N'anti-abuse-switch', N'AntiAbuse_AddAddress_DisableUserFrequencyCheck'),
(N'anti-abuse-switch', N'AntiAbuse_CreateTenancyDetailsSubmission_DisableGlobalFrequencyCheck'),
(N'anti-abuse-switch', N'AntiAbuse_CreateTenancyDetailsSubmission_DisableIpAddressFrequencyCheck'),
(N'anti-abuse-switch', N'AntiAbuse_CreateTenancyDetailsSubmission_DisableUserFrequencyCheck'),
(N'anti-abuse-switch', N'AntiAbuse_PickOutgoingVerification_DisableGlobalFrequencyCheck'),
(N'anti-abuse-switch', N'AntiAbuse_Register_DisableGlobalFrequencyCheck'),
(N'anti-abuse-switch', N'AntiAbuse_Register_DisableIpAddressFrequencyCheck'),
-- anti-abuse-user
(N'anti-abuse-user', N'AntiAbuse_AddAddress_DisableGeocodeFailureUserFrequencyCheck'),
(N'anti-abuse-user', N'AntiAbuse_AddAddress_DisableUserFrequencyCheck'),
(N'anti-abuse-user', N'AntiAbuse_AddAddress_MaxFrequencyPerUser'),
(N'anti-abuse-user', N'AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerUser'),
(N'anti-abuse-user', N'AntiAbuse_CreateTenancyDetailsSubmission_DisableUserFrequencyCheck'),
(N'anti-abuse-user', N'AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser'),
-- debug
(N'debug', N'EnableResponseTiming'),
-- delay-between-retries
(N'delay-between-retries', N'GeocodeService_OverQueryLimitDelayBetweenRetriesInSeconds'),
-- geocode-failure
(N'geocode-failure', N'AntiAbuse_AddAddress_DisableGeocodeFailureIpAddressFrequencyCheck'),
(N'geocode-failure', N'AntiAbuse_AddAddress_DisableGeocodeFailureUserFrequencyCheck'),
(N'geocode-failure', N'AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerIpAddress'),
(N'geocode-failure', N'AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerUser'),
-- geocode-service
(N'geocode-service', N'GeocodeService_OverQueryLimitDelayBetweenRetriesInSeconds'),
(N'geocode-service', N'GeocodeService_OverQueryLimitMaxRetries'),
-- global-max-frequency
(N'global-max-frequency', N'AntiAbuse_AddAddress_GlobalMaxFrequency'),
(N'global-max-frequency', N'AntiAbuse_CreateTenancyDetailsSubmission_GlobalMaxFrequency'),
(N'global-max-frequency', N'AntiAbuse_PickOutgoingVerification_GlobalMaxFrequency'),
(N'global-max-frequency', N'AntiAbuse_Register_GlobalMaxFrequency'),
-- max-retries
(N'max-retries', N'GeocodeService_OverQueryLimitMaxRetries'),
-- global-switch
(N'global-switch', N'AntiAbuse_AddAddress_DisableGlobalFrequencyCheck'),
(N'global-switch', N'AntiAbuse_CreateTenancyDetailsSubmission_DisableGlobalFrequencyCheck'),
(N'global-switch', N'AntiAbuse_Register_DisableGlobalFrequencyCheck'),
(N'global-switch', N'AntiAbuse_PickOutgoingVerification_DisableGlobalFrequencyCheck'),
-- max-frequency
(N'max-frequency', N'AntiAbuse_AddAddress_GlobalMaxFrequency'),
(N'max-frequency', N'AntiAbuse_AddAddress_MaxFrequencyPerIpAddress'),
(N'max-frequency', N'AntiAbuse_AddAddress_MaxFrequencyPerUser'),
(N'max-frequency', N'AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerIpAddress'),
(N'max-frequency', N'AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerUser'),
(N'max-frequency', N'AntiAbuse_CreateTenancyDetailsSubmission_GlobalMaxFrequency'),
(N'max-frequency', N'AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress'),
(N'max-frequency', N'AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser'),
(N'max-frequency', N'AntiAbuse_PickOutgoingVerification_GlobalMaxFrequency'),
(N'max-frequency', N'AntiAbuse_Register_GlobalMaxFrequency'),
(N'max-frequency', N'AntiAbuse_Register_MaxFrequencyPerIpAddress'),
(N'max-frequency', N'TenancyDetailsSubmission_Create_MaxFrequencyPerAddress'),
-- results-limit
(N'results-limit', N'SearchAddressResultsLimit'),
(N'results-limit', N'TenancyDetailsSubmission_MySubmissionsSummary_ItemsLimit'),
-- switch
(N'switch', N'AntiAbuse_AddAddress_DisableGeocodeFailureIpAddressFrequencyCheck'),
(N'switch', N'AntiAbuse_AddAddress_DisableGeocodeFailureUserFrequencyCheck'),
(N'switch', N'AntiAbuse_AddAddress_DisableGlobalFrequencyCheck'),
(N'switch', N'AntiAbuse_AddAddress_DisableIpAddressFrequencyCheck'),
(N'switch', N'AntiAbuse_AddAddress_DisableUserFrequencyCheck'),
(N'switch', N'AntiAbuse_CreateTenancyDetailsSubmission_DisableGlobalFrequencyCheck'),
(N'switch', N'AntiAbuse_CreateTenancyDetailsSubmission_DisableIpAddressFrequencyCheck'),
(N'switch', N'AntiAbuse_CreateTenancyDetailsSubmission_DisableUserFrequencyCheck'),
(N'switch', N'AntiAbuse_PickOutgoingVerification_DisableGlobalFrequencyCheck'),
(N'switch', N'AntiAbuse_Register_DisableIpAddressFrequencyCheck'),
(N'switch', N'EnableResponseTiming'),
(N'switch', N'TenancyDetailsSubmission_Create_DisableFrequencyPerAddressCheck'),
-- tenancy-details-submission
(N'tenancy-details-submission', N'TenancyDetailsSubmission_Create_DisableFrequencyPerAddressCheck'),
(N'tenancy-details-submission', N'TenancyDetailsSubmission_Create_MaxFrequencyPerAddress'),
(N'tenancy-details-submission', N'TenancyDetailsSubmission_MySubmissionsSummary_ItemsLimit');
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