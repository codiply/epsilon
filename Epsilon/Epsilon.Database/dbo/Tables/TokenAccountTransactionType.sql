CREATE TABLE [dbo].[TokenAccountTransactionType] (
    [Id]          NVARCHAR (128) NOT NULL,
    [Description] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.TokenAccountTransactionType] PRIMARY KEY CLUSTERED ([Id] ASC)
);



