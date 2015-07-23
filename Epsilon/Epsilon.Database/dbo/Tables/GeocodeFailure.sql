CREATE TABLE [dbo].[GeocodeFailure] (
    [Id]                 BIGINT             IDENTITY (1, 1) NOT NULL,
    [Address]            NVARCHAR (675)     NULL,
    [CountryId]          NCHAR (2)          NOT NULL,
    [QueryType]          NVARCHAR (MAX)     NULL,
    [FailureType]        NVARCHAR (MAX)     NULL,
    [CreatedOn]          DATETIMEOFFSET (7) NOT NULL,
    [CreatedById]        NVARCHAR (128)     NOT NULL,
    [CreatedByIpAddress] NVARCHAR (39)      NULL,
    CONSTRAINT [PK_dbo.GeocodeFailure] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.GeocodeFailure_dbo.Country_CountryId] FOREIGN KEY ([CountryId]) REFERENCES [dbo].[Country] ([Id]),
    CONSTRAINT [FK_dbo.GeocodeFailure_dbo.User_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[User] ([Id])
);






GO
CREATE NONCLUSTERED INDEX [IX_GeocodeFailure_CreatedOn_CreatedByIpAddress]
    ON [dbo].[GeocodeFailure]([CreatedOn] ASC, [CreatedByIpAddress] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_GeocodeFailure_CreatedOn_CreatedById]
    ON [dbo].[GeocodeFailure]([CreatedById] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_CountryId]
    ON [dbo].[GeocodeFailure]([CountryId] ASC);

