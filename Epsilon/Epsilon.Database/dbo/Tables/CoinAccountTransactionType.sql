CREATE TABLE [dbo].[CoinAccountTransactionType] (
    [Id]          NVARCHAR (128) NOT NULL,
    [Description] NVARCHAR (MAX) NULL,
    [Timestamp]   ROWVERSION     NOT NULL,
    CONSTRAINT [PK_dbo.CoinAccountTransactionType] PRIMARY KEY CLUSTERED ([Id] ASC)
);

