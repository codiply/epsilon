-- PostDeploy\SchemaChanges\DefaultValue.sql START

ALTER TABLE [dbo].[Address] ADD DEFAULT (sysdatetimeoffset()) FOR [CreatedOn]; 
ALTER TABLE [dbo].[AddressGeometry] ADD DEFAULT (sysdatetimeoffset()) FOR [GeocodedOn];
ALTER TABLE [dbo].[AdminAlert] ADD DEFAULT (sysdatetimeoffset()) FOR [SentOn]; 
ALTER TABLE [dbo].[GeocodeFailure] ADD DEFAULT (sysdatetimeoffset()) FOR [MadeOn];
ALTER TABLE [dbo].[IpAddressActivity] ADD DEFAULT (sysdatetimeoffset()) FOR [RecordedOn]; 
ALTER TABLE [dbo].[PostcodeGeometry] ADD DEFAULT (sysdatetimeoffset()) FOR [GeocodedOn];
ALTER TABLE [dbo].[ResponseTiming] ADD DEFAULT (sysdatetimeoffset()) FOR [MeasuredOn];
ALTER TABLE [dbo].[TenancyDetailsSubmission] ADD DEFAULT (sysdatetimeoffset()) FOR [CreatedOn]; 
ALTER TABLE [dbo].[TenantVerification] ADD DEFAULT (sysdatetimeoffset()) FOR [CreatedOn]; 
ALTER TABLE [dbo].[TokenAccount] ADD DEFAULT (sysdatetimeoffset()) FOR [CreatedOn]; 
ALTER TABLE [dbo].[TokenAccountSnapshot] ADD DEFAULT (sysdatetimeoffset()) FOR [MadeOn];
ALTER TABLE [dbo].[TokenAccountTransaction] ADD DEFAULT (sysdatetimeoffset()) FOR [MadeOn]; 

-- PostDeploy\SchemaChanges\DefaultValue.sql END