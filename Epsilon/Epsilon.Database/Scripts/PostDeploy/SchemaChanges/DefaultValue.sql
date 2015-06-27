-- PostDeploy\SchemaChanges\DefaultValue.sql START

ALTER TABLE [dbo].[Address] ADD  DEFAULT (sysdatetimeoffset()) FOR [CreatedOn]
GO

ALTER TABLE [dbo].[AdminAlert] ADD  DEFAULT (sysdatetimeoffset()) FOR [SentOn]
GO

ALTER TABLE [dbo].[TenancyDetailsSubmission] ADD  DEFAULT (sysdatetimeoffset()) FOR [CreatedOn]
GO

-- PostDeploy\SchemaChanges\DefaultValue.sql END