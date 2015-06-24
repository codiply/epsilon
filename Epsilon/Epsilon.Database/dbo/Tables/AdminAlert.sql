CREATE TABLE [dbo].[AdminAlert] (
    [Id]     UNIQUEIDENTIFIER   DEFAULT (newsequentialid()) NOT NULL,
    [Key]    NVARCHAR (256)     NOT NULL,
    [SentOn] DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_dbo.AdminAlert] PRIMARY KEY CLUSTERED ([Id] ASC)
);

