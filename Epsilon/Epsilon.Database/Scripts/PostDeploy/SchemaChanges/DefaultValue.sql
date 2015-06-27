-- PostDeploy\SchemaChanges\DefaultValue.sql START

ALTER TABLE [dbo].[Address] ADD DEFAULT (sysdatetimeoffset()) FOR [CreatedOn]; GO
ALTER TABLE [dbo].[AdminAlert] ADD DEFAULT (sysdatetimeoffset()) FOR [SentOn]; GO
ALTER TABLE [dbo].[CoinAccount] ADD DEFAULT (sysdatetimeoffset()) FOR [CreatedOn]; GO
ALTER TABLE [dbo].[CoinAccountSnapshot] ADD DEFAULT (sysdatetimeoffset()) FOR [CreatedOn]; GO
ALTER TABLE [dbo].[CoinAccountTransaction] ADD DEFAULT (sysdatetimeoffset()) FOR [TookPlaceOn]; GO
ALTER TABLE [dbo].[TenancyDetailsSubmission] ADD DEFAULT (sysdatetimeoffset()) FOR [CreatedOn]; GO

-- PostDeploy\SchemaChanges\DefaultValue.sql END