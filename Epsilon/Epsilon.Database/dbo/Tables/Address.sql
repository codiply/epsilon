CREATE TABLE [dbo].[Address] (
    [Id]        NVARCHAR (128) NOT NULL,
    [Timestamp] ROWVERSION     NOT NULL,
    CONSTRAINT [PK_dbo.Address] PRIMARY KEY CLUSTERED ([Id] ASC)
);

