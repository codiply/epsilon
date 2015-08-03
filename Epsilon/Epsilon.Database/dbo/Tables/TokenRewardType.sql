CREATE TABLE [dbo].[TokenRewardType] (
    [Key]         NVARCHAR (128) NOT NULL,
    [Description] NVARCHAR (256) NULL,
    CONSTRAINT [PK_dbo.TokenRewardType] PRIMARY KEY CLUSTERED ([Key] ASC)
);

