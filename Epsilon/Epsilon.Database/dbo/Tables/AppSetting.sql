CREATE TABLE [dbo].[AppSetting] (
    [Id]          NVARCHAR (128)     NOT NULL,
    [Value]       NVARCHAR (MAX)     NOT NULL,
    [ValueType]   NVARCHAR (16)      NOT NULL,
    [Description] NVARCHAR (MAX)     NULL,
    [UpdatedById] NVARCHAR (128)     NULL,
    [UpdatedOn]   DATETIMEOFFSET (7) NULL,
    [Timestamp]   ROWVERSION         NOT NULL,
    CONSTRAINT [PK_dbo.AppSetting] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.AppSetting_dbo.User_UpdatedById] FOREIGN KEY ([UpdatedById]) REFERENCES [dbo].[User] ([Id])
);










GO
CREATE NONCLUSTERED INDEX [IX_UpdatedById]
    ON [dbo].[AppSetting]([UpdatedById] ASC);

