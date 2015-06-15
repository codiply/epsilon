CREATE TABLE [dbo].[Country] (
    [Id]          NCHAR (2)     NOT NULL,
    [EnglishName] NVARCHAR (64) NULL,
    CONSTRAINT [PK_dbo.Country] PRIMARY KEY CLUSTERED ([Id] ASC)
);



