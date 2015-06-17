CREATE TABLE [dbo].[Currency] (
    [Id]          NCHAR (3)     NOT NULL,
    [EnglishName] NVARCHAR (64) NULL,
    [Symbol]      NVARCHAR (4)  NULL,
    [Timestamp]   ROWVERSION    NOT NULL,
    CONSTRAINT [PK_dbo.Currency] PRIMARY KEY CLUSTERED ([Id] ASC)
);

