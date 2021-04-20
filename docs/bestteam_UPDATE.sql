-- update checkInCount in Business 
UPDATE Business
SET checkInCount = Temp.numChecks
FROM (
        SELECT business.businessID,
            COUNT(ChecksIn.businessID) as numChecks
        FROM Business
            LEFT JOIN ChecksIn ON Business.businessID = ChecksIn.businessID
        GROUP BY business.businessID
    ) as Temp
WHERE Business.businessID = Temp.businessID;
-- update tipCount in Business 
UPDATE Business
SET tipCount = Temp.numTips
FROM (
        SELECT business.businessID,
            COUNT(tip.businessID) as numTips
        FROM Business
            LEFT JOIN Tip ON Business.businessID = Tip.businessID
        GROUP BY business.businessID
    ) as Temp
WHERE Business.businessID = Temp.businessID;
-- update totalLikes in Users
UPDATE Users
SET totalLikes = Temp.sumLikes
FROM (
        SELECT userID, SUM(likes) as sumLikes
        FROM Tip
        GROUP BY userID
    ) as Temp
WHERE Users.userID = Temp.userID;
-- update tipCount in Users 
UPDATE Users
SET tipCount = Temp.numTips
FROM (
        SELECT Users.userID,
            COUNT(Tip.userID) as numTips
        FROM Users
            LEFT JOIN Tip ON Users.userID = Tip.userID
        GROUP BY Users.userID
    ) as Temp
WHERE Users.userID = Temp.userID;