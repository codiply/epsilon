CREATE TABLE [dbo].[TokenAccountTransaction] (
    [Id]        BIGINT             IDENTITY (1, 1) NOT NULL,
    [AccountId] NVARCHAR (128)     NOT NULL,
    [TypeId]    NVARCHAR (128)     NOT NULL,
    [Amount]    DECIMAL (18, 2)    NOT NULL,
    [MadeOn]    DATETIMEOFFSET (7) NOT NULL,
    [Reference] NVARCHAR (256)     NULL,
    CONSTRAINT [PK_dbo.TokenAccountTransaction] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.TokenAccountTransaction_dbo.TokenAccount_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[TokenAccount] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.TokenAccountTransaction_dbo.TokenAccountTransactionType_TypeId] FOREIGN KEY ([TypeId]) REFERENCES [dbo].[TokenAccountTransactionType] ([Id])
);










GO
CREATE NONCLUSTERED INDEX [IX_TypeId]
    ON [dbo].[TokenAccountTransaction]([TypeId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TokenAccountTransaction_AccountId_MadeOn]
    ON [dbo].[TokenAccountTransaction]([AccountId] ASC, [MadeOn] ASC);

