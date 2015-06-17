CREATE TABLE [dbo].[Country] (
    [Id]            NCHAR (2)     NOT NULL,
    [EnglishName]   NVARCHAR (64) NULL,
    [LocalizedName] NVARCHAR (64) NULL,
    [CurrencyId]    NCHAR (3)     NOT NULL,
    [IsAvailable]   BIT           NOT NULL,
    [Timestamp]     ROWVERSION    NOT NULL,
    CONSTRAINT [PK_dbo.Country] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.Country_dbo.Currency_CurrencyId] FOREIGN KEY ([CurrencyId]) REFERENCES [dbo].[Currency] ([Id]) ON DELETE CASCADE
);








GO
CREATE NONCLUSTERED INDEX [IX_CurrencyId]
    ON [dbo].[Country]([CurrencyId] ASC);

