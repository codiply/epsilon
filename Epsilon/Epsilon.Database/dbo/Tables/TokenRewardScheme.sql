CREATE TABLE [dbo].[TokenRewardScheme] (
    [Id]            INT                IDENTITY (1, 1) NOT NULL,
    [EffectiveFrom] DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_dbo.TokenRewardScheme] PRIMARY KEY CLUSTERED ([Id] ASC)
);

