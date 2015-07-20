CREATE TABLE [dbo].[TokenReward] (
    [SchemeId] INT             NOT NULL,
    [Key]      NVARCHAR (128)  NOT NULL,
    [Value]    DECIMAL (16, 4) NOT NULL,
    CONSTRAINT [PK_dbo.TokenReward] PRIMARY KEY CLUSTERED ([SchemeId] ASC, [Key] ASC),
    CONSTRAINT [FK_dbo.TokenReward_dbo.TokenRewardScheme_SchemeId] FOREIGN KEY ([SchemeId]) REFERENCES [dbo].[TokenRewardScheme] ([Id]) ON DELETE CASCADE
);




GO
CREATE NONCLUSTERED INDEX [IX_SchemeId]
    ON [dbo].[TokenReward]([SchemeId] ASC);

