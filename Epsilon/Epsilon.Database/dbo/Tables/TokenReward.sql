CREATE TABLE [dbo].[TokenReward] (
    [SchemeId] INT             NOT NULL,
    [TypeKey]  NVARCHAR (128)  NOT NULL,
    [Value]    DECIMAL (16, 4) NOT NULL,
    CONSTRAINT [PK_dbo.TokenReward] PRIMARY KEY CLUSTERED ([SchemeId] ASC, [TypeKey] ASC),
    CONSTRAINT [FK_dbo.TokenReward_dbo.TokenRewardScheme_SchemeId] FOREIGN KEY ([SchemeId]) REFERENCES [dbo].[TokenRewardScheme] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.TokenReward_dbo.TokenRewardType_TypeKey] FOREIGN KEY ([TypeKey]) REFERENCES [dbo].[TokenRewardType] ([Key]) ON DELETE CASCADE
);






GO
CREATE NONCLUSTERED INDEX [IX_SchemeId]
    ON [dbo].[TokenReward]([SchemeId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TypeKey]
    ON [dbo].[TokenReward]([TypeKey] ASC);

