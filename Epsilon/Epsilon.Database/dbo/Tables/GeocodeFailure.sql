CREATE TABLE [dbo].[GeocodeFailure] (
    [Id]              BIGINT             IDENTITY (1, 1) NOT NULL,
    [Address]         NVARCHAR (MAX)     NULL,
    [Region]          NVARCHAR (MAX)     NULL,
    [QueryType]       NVARCHAR (MAX)     NULL,
    [FailureType]     NVARCHAR (MAX)     NULL,
    [MadeOn]          DATETIMEOFFSET (7) NOT NULL,
    [MadeById]        NVARCHAR (128)     NOT NULL,
    [MadeByIpAddress] NVARCHAR (39)      NULL,
    CONSTRAINT [PK_dbo.GeocodeFailure] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.GeocodeFailure_dbo.User_MadeById] FOREIGN KEY ([MadeById]) REFERENCES [dbo].[User] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_MadeById]
    ON [dbo].[GeocodeFailure]([MadeById] ASC);

