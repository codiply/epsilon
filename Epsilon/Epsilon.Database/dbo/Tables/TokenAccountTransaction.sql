CREATE TABLE [dbo].[TokenAccountTransaction] (
    [Id]                BIGINT             IDENTITY (1, 1) NOT NULL,
    [UniqueId]          UNIQUEIDENTIFIER   NOT NULL,
    [AccountId]         NVARCHAR (128)     NOT NULL,
    [RewardTypeKey]     NVARCHAR (128)     NOT NULL,
    [Amount]            DECIMAL (16, 4)    NOT NULL,
    [Quantity]          INT                NOT NULL,
    [MadeOn]            DATETIMEOFFSET (7) NOT NULL,
    [InternalReference] UNIQUEIDENTIFIER   NULL,
    [ExternalReference] NVARCHAR (256)     NULL,
    CONSTRAINT [PK_dbo.TokenAccountTransaction] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.TokenAccountTransaction_dbo.TokenAccount_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[TokenAccount] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.TokenAccountTransaction_dbo.TokenRewardType_RewardTypeKey] FOREIGN KEY ([RewardTypeKey]) REFERENCES [dbo].[TokenRewardType] ([Key])
);




















GO



GO
CREATE NONCLUSTERED INDEX [IX_TokenAccountTransaction_MadeOn_AccountId]
    ON [dbo].[TokenAccountTransaction]([MadeOn] ASC, [AccountId] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TokenAccountTransaction_UniqueId]
    ON [dbo].[TokenAccountTransaction]([UniqueId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_RewardTypeKey]
    ON [dbo].[TokenAccountTransaction]([RewardTypeKey] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TokenAccountTransaction_AccountId_MadeOn]
    ON [dbo].[TokenAccountTransaction]([AccountId] ASC, [MadeOn] ASC);

