
-- Update AND Trigger for up-to-date number of checkins in the Business table 
CREATE OR REPLACE FUNCTION UpdateNumCheckins() RETURNS trigger AS '
BEGIN
    UPDATE Business 
    SET checkInCount = checkInCount + 1
    WHERE Business.businessID = NEW.businessID
    RETURN NEW
END
' LANGUAGE plpgsql;

CREATE TRIGGER NewCheckIn
AFTER INSERT ON ChecksIn
FOR EACH STATEMENT 
EXECUTE PROCEDURE UpdateNumCheckins(); 

-- Update for up-to-date number of tips written about a business in Buisness table 
CREATE OR REPLACE FUNCTION UpdateNumTips() RETURNS trigger AS '
BEGIN
    UPDATE Business 
    SET numTips = numTips + 1
    WHERE Business.businessID = NEW.businessID
    RETURN NEW
END
' LANGUAGE plpgsql;

-- Update for up-to-date total number of likes in the Users table 
CREATE OR REPLACE FUNCTION UpdateTotalLikes() RETURNS trigger AS '
BEGIN
    UPDATE Users 
    SET totalLikes = totalLikes + NEW.likes
    WHERE User.userID = NEW.userID
    RETURN NEW
END
' LANGUAGE plpgsql;

-- Update for up-to-date number of tips written by a user in the Users table 
CREATE OR REPLACE FUNCTION UpdateTipCount() RETURNS trigger AS '
BEGIN
    UPDATE Users 
    SET tipCount = tipCount + 1
    WHERE User.userID = NEW.userID
    RETURN NEW
END
' LANGUAGE plpgsql;

-- Function combining UpdateNumTips, UpdateTotalLikes, and UpdateTipCount to be ran with the same trigger
CREATE OR REPLACE FUNCTION TipInfoUpdate() RETURNS trigger AS '
BEGIN
    PERFORM UpdateNumTips()
    PERFORM UpdateTotalLikes()
    PERFORM UpdateTipCount()
END
' LANGUAGE plpgsql;

-- Trigger for updating tipCount in Business and Users, and totalLikes in Users
CREATE TRIGGER NewTip
AFTER INSERT ON ChecksIn
FOR EACH STATEMENT 
EXECUTE PROCEDURE TipInfoUpdate(); 

-- Test code for NewCheckIn Trigger
SELECT businessID, checkInCount FROM Buisness WHERE buisnessID = 'gnKjwL_1w79qoiV3IC_xQQ' ;
INSERT INTO ChecksIn VALUES ('gnKjwL_1w79qoiV3IC_xQQ','2021-03-23 12:00:00');
SELECT businessID, checkInCount FROM Buisness WHERE buisnessID = 'gnKjwL_1w79qoiV3IC_xQQ' ;
-- clean 
DROP TRIGGER UpdateNumCheckins on Buisness;

-- Test code for NewTip Trigger
SELECT businessID, numTips FROM Buisness WHERE buisnessID = 'gnKjwL_1w79qoiV3IC_xQQ' ;
SELECT userID, tipCount, totalLikes FROM Users WHERE userID = 'TEtzbpgA2BFBrC0y0sCbfw';
INSERT INTO Tip VALUES ('TEtzbpgA2BFBrC0y0sCbfw','gnKjwL_1w79qoiV3IC_xQQ','2021-03-23 12:00:00', '10', 'Good Atmosphere!');
SELECT businessID, numTips FROM Buisness WHERE buisnessID = 'gnKjwL_1w79qoiV3IC_xQQ' ;
SELECT userID, tipCount, totalLikes FROM Users WHERE userID = 'TEtzbpgA2BFBrC0y0sCbfw';
-- clean 
DROP TRIGGER NewTip on Buisness;
DROP TRIGGER NewTip on Users;