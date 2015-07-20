CREATE TABLE [dbo].[CoinAccountTransaction] (
    [Id]        BIGINT             IDENTITY (1, 1) NOT NULL,
    [AccountId] NVARCHAR (128)     NOT NULL,
    [TypeId]    NVARCHAR (128)     NOT NULL,
    [Amount]    DECIMAL (18, 2)    NOT NULL,
    [MadeOn]    DATETIMEOFFSET (7) NOT NULL,
    [Reference] NVARCHAR (256)     NULL,
    [Timestamp] ROWVERSION         NOT NULL,
    CONSTRAINT [PK_dbo.CoinAccountTransaction] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.CoinAccountTransaction_dbo.CoinAccount_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[CoinAccount] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.CoinAccountTransaction_dbo.CoinAccountTransactionType_TypeId] FOREIGN KEY ([TypeId]) REFERENCES [dbo].[CoinAccountTransactionType] ([Id])
);








GO
CREATE NONCLUSTERED INDEX [IX_TypeId]
    ON [dbo].[CoinAccountTransaction]([TypeId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_CoinAccountTransaction_AccountId_MadeOn]
    ON [dbo].[CoinAccountTransaction]([AccountId] ASC, [MadeOn] ASC);

