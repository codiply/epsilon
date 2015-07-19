﻿CREATE TABLE [dbo].[AddressGeometry] (
    [Id]                         BIGINT             IDENTITY (1, 1) NOT NULL,
    [Latitude]                   FLOAT (53)         NOT NULL,
    [Longitude]                  FLOAT (53)         NOT NULL,
    [ViewportNortheastLatitude]  FLOAT (53)         NOT NULL,
    [ViewportNortheastLongitude] FLOAT (53)         NOT NULL,
    [ViewportSouthwestLatitude]  FLOAT (53)         NOT NULL,
    [ViewportSouthwestLongitude] FLOAT (53)         NOT NULL,
    [AddressId]                  BIGINT             NOT NULL,
    [GeocodedOn]                 DATETIMEOFFSET (7) NOT NULL,
    [Timestamp]                  ROWVERSION         NOT NULL,
    CONSTRAINT [PK_dbo.AddressGeometry] PRIMARY KEY CLUSTERED ([Id] ASC)
);

