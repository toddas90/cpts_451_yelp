
-- update checkInCount in Business 
UPDATE Business, 
    (SELECT businessID, COUNT(checkInDate) as numCheckIns 
         FROM ChecksIn
         GROUP BY businessID
    ) as Temp
SET Business.checkInCount = Temp.numCheckIns
WHERE Business.businessID = Temp.businessID
;

-- update tipCount in Business 
UPDATE Business, 
    (SELECT businessID, COUNT(userID) as numTips 
         FROM Tip
         GROUP BY businessID
    ) as Temp
SET Business.tipCount = Temp.numTips
WHERE Business.businessID = Temp.businessID
;

-- update totalLikes in Users
UPDATE Users, 
    (SELECT userID, buisnessID SUM(likes) as sumLikes 
         FROM Tip
         GROUP BY userID
    ) as Temp
SET Users.totalLikes = Temp.sumLikes
WHERE Users.userID = Temp.userID
;

-- update tipCount in Users 
UPDATE Users, 
    (SELECT userID, buisnessID COUNT(dateWritten) as numTips
         FROM Tip
         GROUP BY userID
    ) as Temp
SET Users.tipCount = Temp.numTips
WHERE Users.userID = Temp.userID
;
