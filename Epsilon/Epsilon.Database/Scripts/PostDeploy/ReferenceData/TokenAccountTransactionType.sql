﻿-- PostDeploy\ReferenceData\TokenAccountTransactionType.sql START

-- NOTE 1: The id should be UPERCASE.
-- NOTE 2: All the Id's defined here should also be added to the TokenAccountTransactionTypeId enum.

GO
-- I drop the temporary table  #TMP if it exists.
IF OBJECT_ID('tempdb..#TMP') IS NOT NULL DROP TABLE #TMP
GO

-- I copy the schema of the source table into #TMP.
SELECT * INTO #TMP
FROM [dbo].[TokenAccountTransactionType]
WHERE 1 = 0

INSERT INTO #TMP
([Id], [Description])
VALUES
-- Edit the values below to update the target table.
(N'CREDIT', N'Credit tokens to account.'),
(N'DEBIT', N'Debit tokens to account.');
GO

MERGE [dbo].[TokenAccountTransactionType] AS T -- Target
USING #TMP AS S -- Source
    ON T.Id = S.Id
WHEN MATCHED
    THEN UPDATE SET
	    T.[Description] = S.[Description]
WHEN NOT MATCHED
    THEN INSERT ([Id], [Description])
	VALUES (S.[Id], S.[Description])
WHEN NOT MATCHED BY SOURCE 
    THEN DELETE;
GO

DROP TABLE #TMP;
GO

-- PostDeploy\ReferenceData\TokenAccountTransactionType.sql END