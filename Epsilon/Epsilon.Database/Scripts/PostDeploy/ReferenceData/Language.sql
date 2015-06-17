-- PostDeploy\ReferenceData\Language.sql START

-- Currency codes and symbols can be found here:
-- http://www.xe.com/symbols.php

GO

INSERT INTO [dbo].[Language]
([Id], [EnglishName], [LocalizedName], [IsAvailable])
VALUES
(N'el-GR', N'Greek', N'Ελληνικά', 1);

GO

-- PostDeploy\ReferenceData\Language.sql END