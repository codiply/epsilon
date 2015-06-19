CREATE TABLE [dbo].[Language] (
    [Id]          NVARCHAR (8)  NOT NULL,
    [EnglishName] NVARCHAR (64) NULL,
    [LocalName]  NVARCHAR (64) NULL,
    [CultureCode] NVARCHAR (10) NULL,
    [IsAvailable] BIT           NOT NULL,
    CONSTRAINT [PK_dbo.Language] PRIMARY KEY CLUSTERED ([Id] ASC)
);








GO


