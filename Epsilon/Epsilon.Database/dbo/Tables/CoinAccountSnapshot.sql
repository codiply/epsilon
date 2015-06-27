CREATE TABLE [dbo].[CoinAccountSnapshot] (
    [Id]          UNIQUEIDENTIFIER   DEFAULT (newsequentialid()) NOT NULL,
    [AccountId]   NVARCHAR (128)     NOT NULL,
    [Balance]     DECIMAL (18, 2)    NOT NULL,
    [MadeOn]      DATETIMEOFFSET (7) NOT NULL,
    [IsFinalised] BIT                NOT NULL,
    [Timestamp]   ROWVERSION         NOT NULL,
    CONSTRAINT [PK_dbo.CoinAccountSnapshot] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.CoinAccountSnapshot_dbo.CoinAccount_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[CoinAccount] ([Id]) ON DELETE CASCADE
);






GO
CREATE NONCLUSTERED INDEX [AccountId_MadeOn_IsFinalised]
    ON [dbo].[CoinAccountSnapshot]([AccountId] ASC, [MadeOn] ASC, [IsFinalised] ASC);

