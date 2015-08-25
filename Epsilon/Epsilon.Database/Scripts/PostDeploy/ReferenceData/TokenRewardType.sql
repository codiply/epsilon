-- PostDeploy\ReferenceData\TokenRewardType.sql START

-- NOTE: All the Id's defined here should also be added to the TokenRewardKey enum.

GO
-- I drop the temporary table  #TMP if it exists.
IF OBJECT_ID('tempdb..#TMP') IS NOT NULL DROP TABLE #TMP
GO

-- I copy the schema of the source table into #TMP.
SELECT * INTO #TMP
FROM [dbo].[TokenRewardType]
WHERE 1 = 0


INSERT INTO #TMP
([Key], [Description])
VALUES
-- Edit the values below to update the target table.
-- Earn
(N'EarnPerTenancyDetailsSubmission', N'Tokens earned when user submit details for their tenancy.'),
(N'EarnPerVerificationCodeEntered', N'Tokens earned when a user enters a verification code received by post.'),
(N'EarnPerVerificationLuckySender', N'Tokens earned in random by the sender of a verification, when the recipient enters the verification code. The probability is 0.01 and it depends on the milliseconds of the time when the code is entered.'),
(N'EarnPerVerificationMailSent', N'Tokens earned when a user sends verification post and the code is entered by the recepient.'),
-- Spend
(N'SpendPerPropertyInfoAccess', N'Tokens spent when user buys access to the information of a property.')
GO

MERGE [dbo].[TokenRewardType] AS T -- Target
USING #TMP AS S -- Source
    ON T.[Key] = S.[Key]
WHEN MATCHED
    THEN UPDATE SET
        T.[Description] = S.[Description]
WHEN NOT MATCHED
    THEN INSERT ([Key], [Description])
    VALUES (S.[Key], S.[Description])
WHEN NOT MATCHED BY SOURCE 
    THEN DELETE;
GO

DROP TABLE #TMP;
GO

-- PostDeploy\ReferenceData\TokenRewardType.sql END