CREATE TABLE [dbo].[ResponseTiming] (
    [Id]                 BIGINT             IDENTITY (1, 1) NOT NULL,
    [MeasuredOn]         DATETIMEOFFSET (7) NOT NULL,
    [ControllerName]     NVARCHAR (128)     NOT NULL,
    [ActionName]         NVARCHAR (128)     NOT NULL,
    [IsApi]              BIT                NOT NULL,
    [TimeInMilliseconds] INT                NOT NULL,
    CONSTRAINT [PK_dbo.ResponseTiming] PRIMARY KEY CLUSTERED ([Id] ASC)
);

