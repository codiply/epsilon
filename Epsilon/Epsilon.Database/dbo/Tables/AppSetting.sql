CREATE TABLE [dbo].[AppSetting] (
    [Id]          NVARCHAR (128) NOT NULL,
    [Value]       NVARCHAR (MAX) NOT NULL,
    [ValueType]   NVARCHAR (16)  NOT NULL,
    [Description] NVARCHAR (MAX) NULL,
    [Timestamp]   ROWVERSION     NOT NULL,
    CONSTRAINT [PK_dbo.AppSetting] PRIMARY KEY CLUSTERED ([Id] ASC)
);





