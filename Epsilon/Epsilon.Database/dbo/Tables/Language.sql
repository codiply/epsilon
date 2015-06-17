CREATE TABLE [dbo].[Language] (
    [Id]            NCHAR (10)     NOT NULL,
    [EnglishName]   NVARCHAR (64) NULL,
    [LocalizedName] NVARCHAR (64) NULL,
    [IsAvailable]   BIT           NOT NULL,
    CONSTRAINT [PK_dbo.Language] PRIMARY KEY CLUSTERED ([Id] ASC)
);

