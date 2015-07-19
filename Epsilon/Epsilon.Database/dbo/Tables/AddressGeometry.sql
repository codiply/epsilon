CREATE TABLE [dbo].[AddressGeometry] (
    [Id]                         BIGINT             NOT NULL,
    [Latitude]                   FLOAT (53)         NOT NULL,
    [Longitude]                  FLOAT (53)         NOT NULL,
    [ViewportNortheastLatitude]  FLOAT (53)         NOT NULL,
    [ViewportNortheastLongitude] FLOAT (53)         NOT NULL,
    [ViewportSouthwestLatitude]  FLOAT (53)         NOT NULL,
    [ViewportSouthwestLongitude] FLOAT (53)         NOT NULL,
    [GeocodedOn]                 DATETIMEOFFSET (7) NOT NULL,
    [Timestamp]                  ROWVERSION         NOT NULL,
    CONSTRAINT [PK_dbo.AddressGeometry] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.AddressGeometry_dbo.Address_Id] FOREIGN KEY ([Id]) REFERENCES [dbo].[Address] ([Id]) ON DELETE CASCADE
);




GO
CREATE NONCLUSTERED INDEX [IX_Id]
    ON [dbo].[AddressGeometry]([Id] ASC);

