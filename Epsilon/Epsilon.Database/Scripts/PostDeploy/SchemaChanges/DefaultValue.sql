﻿-- PostDeploy\SchemaChanges\DefaultValue.sql START

ALTER TABLE [dbo].[Address] ADD DEFAULT (sysdatetimeoffset()) FOR [CreatedOn]; 
ALTER TABLE [dbo].[AdminAlert] ADD DEFAULT (sysdatetimeoffset()) FOR [SentOn]; 
ALTER TABLE [dbo].[CoinAccount] ADD DEFAULT (sysdatetimeoffset()) FOR [CreatedOn]; 
ALTER TABLE [dbo].[CoinAccountSnapshot] ADD DEFAULT (sysdatetimeoffset()) FOR [MadeOn];
ALTER TABLE [dbo].[CoinAccountTransaction] ADD DEFAULT (sysdatetimeoffset()) FOR [MadeOn]; 
ALTER TABLE [dbo].[TenancyDetailsSubmission] ADD DEFAULT (sysdatetimeoffset()) FOR [CreatedOn]; 


-- PostDeploy\SchemaChanges\DefaultValue.sql END