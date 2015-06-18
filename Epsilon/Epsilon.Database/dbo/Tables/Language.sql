CREATE TABLE [dbo].[Language] (
    [Id]            NVARCHAR (10) NOT NULL,
    [EnglishName]   NVARCHAR (64) NULL,
    [NativeName]    NVARCHAR (64) NULL,
    [UseLanguageId] NVARCHAR (10) NULL,
    [IsAvailable]   BIT           NOT NULL,
    CONSTRAINT [PK_dbo.Language] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.Language_dbo.Language_UseLanguageId] FOREIGN KEY ([UseLanguageId]) REFERENCES [dbo].[Language] ([Id])
);






GO
CREATE NONCLUSTERED INDEX [IX_UseLanguageId]
    ON [dbo].[Language]([UseLanguageId] ASC);

