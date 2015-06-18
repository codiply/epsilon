-- PostDeploy\ReferenceData\Language.sql START

-- Currency codes and symbols can be found here:
-- http://www.xe.com/symbols.php

GO

INSERT INTO [dbo].[Language]
([Id], [EnglishName], [NativeName], [IsAvailable], [UseLanguageId])
VALUES
(N'en-GB', N'English', N'English', 1, NULL),
(N'en-US', N'English', N'English', 1, N'en-GB'),
(N'el-GR', N'Greek', N'Ελληνικά', 1, NULL);

GO

-- PostDeploy\ReferenceData\Language.sql END