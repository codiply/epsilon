CREATE TABLE [dbo].[AppSettingLabel] (
    [AppSettingId] NVARCHAR (128) NOT NULL,
    [Label]        NVARCHAR (128) NOT NULL,
    CONSTRAINT [PK_dbo.AppSettingLabel] PRIMARY KEY CLUSTERED ([AppSettingId] ASC, [Label] ASC),
    CONSTRAINT [FK_dbo.AppSettingLabel_dbo.AppSetting_AppSettingId] FOREIGN KEY ([AppSettingId]) REFERENCES [dbo].[AppSetting] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_AppSettingId]
    ON [dbo].[AppSettingLabel]([AppSettingId] ASC);

