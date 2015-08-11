CREATE TABLE [dbo].[Country] (
    [Id]             NCHAR (2)     NOT NULL,
    [EnglishName]    NVARCHAR (64) NULL,
    [LocalName]      NVARCHAR (64) NULL,
    [CurrencyId]     NCHAR (3)     NOT NULL,
    [MainLanguageId] NVARCHAR (8)  NOT NULL,
    [IsAvailable]    BIT           NOT NULL,
    [Timestamp]      ROWVERSION    NOT NULL,
    CONSTRAINT [PK_dbo.Country] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.Country_dbo.Currency_CurrencyId] FOREIGN KEY ([CurrencyId]) REFERENCES [dbo].[Currency] ([Id]),
    CONSTRAINT [FK_dbo.Country_dbo.Language_MainLanguageId] FOREIGN KEY ([MainLanguageId]) REFERENCES [dbo].[Language] ([Id])
);














GO
CREATE NONCLUSTERED INDEX [IX_CurrencyId]
    ON [dbo].[Country]([CurrencyId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_MainLanguageId]
    ON [dbo].[Country]([MainLanguageId] ASC);

