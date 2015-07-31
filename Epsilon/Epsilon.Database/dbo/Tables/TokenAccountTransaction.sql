CREATE TABLE [dbo].[TokenAccountTransaction] (
    [Id]        BIGINT             IDENTITY (1, 1) NOT NULL,
    [UniqueId]  UNIQUEIDENTIFIER   NOT NULL,
    [AccountId] NVARCHAR (128)     NOT NULL,
    [TypeId]    NVARCHAR (128)     NOT NULL,
    [Amount]    DECIMAL (16, 4)    NOT NULL,
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
CREATE NONCLUSTERED INDEX [IX_TokenAccountTransaction_MadeOn_AccountId]
    ON [dbo].[TokenAccountTransaction]([MadeOn] ASC, [AccountId] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TokenAccountTransaction_UniqueId]
    ON [dbo].[TokenAccountTransaction]([UniqueId] ASC);

