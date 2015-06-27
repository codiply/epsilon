﻿/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

:r .\SchemaChanges\DefaultValue.sql

:r .\ReferenceData\AppSetting.sql
:r .\ReferenceData\AspNetRoles.sql
:r .\ReferenceData\Currency.sql
:r .\ReferenceData\CoinAccountTransactionType.sql
:r .\ReferenceData\Country.sql
:r .\ReferenceData\Language.sql
GO