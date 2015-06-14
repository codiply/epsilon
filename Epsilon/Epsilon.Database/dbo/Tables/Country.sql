CREATE TABLE [dbo].[Country] (
    [Id]          NVARCHAR (128) NOT NULL,
    [EnglishName] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.Country] PRIMARY KEY CLUSTERED ([Id] ASC)
);

