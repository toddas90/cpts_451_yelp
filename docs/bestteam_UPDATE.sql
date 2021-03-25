-- update checkInCount in Business 
UPDATE Business
SET checkInCount = Temp.numCheckIns
FROM (
        SELECT businessID,
            COUNT(checkInDate) as numCheckIns
        FROM ChecksIn
        GROUP BY businessID
    ) as Temp
WHERE Business.businessID = Temp.businessID;
-- update tipCount in Business 
UPDATE Business
SET tipCount = Temp.numTips
FROM (
        SELECT businessID,
            COUNT(userID) as numTips
        FROM Tip
        GROUP BY businessID
    ) as Temp
WHERE Business.businessID = Temp.businessID;
-- update totalLikes in Users
UPDATE Users
SET totalLikes = Temp.sumLikes
FROM (
        SELECT userID,
            businessID,
            SUM(likes) as sumLikes
        FROM Tip
        GROUP BY userID,
            businessID
    ) as Temp
WHERE Users.userID = Temp.userID;
-- update tipCount in Users 
UPDATE Users
SET tipCount = Temp.numTips
FROM (
        SELECT userID,
            businessID,
            COUNT(dateWritten) as numTips
        FROM Tip
        GROUP BY userID,
            businessID
    ) as Temp
WHERE Users.userID = Temp.userID;