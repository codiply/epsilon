CREATE TABLE [dbo].[ResponseTiming] (
    [Id]                 BIGINT             IDENTITY (1, 1) NOT NULL,
    [MeasuredOn]         DATETIMEOFFSET (7) NOT NULL,
    [LanguageId]         NVARCHAR (8)       NOT NULL,
    [ControllerName]     NVARCHAR (128)     NOT NULL,
    [ActionName]         NVARCHAR (128)     NULL,
    [HttpVerb]           NVARCHAR (8)       NOT NULL,
    [IsApi]              BIT                NOT NULL,
    [TimeInMilliseconds] FLOAT (53)         NOT NULL,
    CONSTRAINT [PK_dbo.ResponseTiming] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ResponseTiming_dbo.Language_LanguageId] FOREIGN KEY ([LanguageId]) REFERENCES [dbo].[Language] ([Id]) ON DELETE CASCADE
);






GO
CREATE NONCLUSTERED INDEX [IX_LanguageId]
    ON [dbo].[ResponseTiming]([LanguageId] ASC);

