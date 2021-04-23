-- Function to maintain the checkInCount for each businessID in Business
CREATE OR REPLACE FUNCTION UpdateCheckInCount() RETURNS trigger AS $$ 
BEGIN 
   UPDATE  Business   
   SET  checkInCount = checkInCount+1   
   WHERE Business.businessID=NEW.businessID;   
   RETURN NEW;
END;
$$ LANGUAGE plpgsql;


-- Function combining UpdateNumTips, UpdateTotalLikes, and UpdateTipCount to be ran with the same trigger
CREATE OR REPLACE FUNCTION UpdateTipCount() RETURNS trigger AS $$ 
BEGIN 
    UPDATE Users
    SET tipCount = tipCount + 1
    WHERE Users.userID = NEW.userID;
    UPDATE Business
    SET tipCount = tipCount + 1
    WHERE Business.businessID = NEW.businessID;
    RETURN NEW;

END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION UpdateTipLikeCount() RETURNS trigger AS $$ 
BEGIN 
    UPDATE Users
    SET totalLikes = totalLikes + 1
    WHERE Users.userID = NEW.userID;
    UPDATE Tip
    SET likes = likes + 1
    WHERE Tip.businessID = NEW.businessID AND Tip.userID = NEW.userID AND Tip.dateWritten = NEW.dateWritten;
    RETURN NEW;

END;
$$ LANGUAGE plpgsql;

-- Trigger for updating tipCount in Business and Users, and totalLikes in Users
CREATE TRIGGER NewTip
AFTER INSERT ON Tip 
FOR EACH ROW 
EXECUTE FUNCTION UpdateTipCount();

CREATE TRIGGER UpdateTipLikes
AFTER UPDATE ON Tip 
FOR EACH ROW 
EXECUTE FUNCTION UpdateTipLikeCount();

-- Trigger for updating checkInCount in Business
CREATE TRIGGER NewCheckIn
AFTER INSERT ON ChecksIn 
FOR EACH ROW 
EXECUTE FUNCTION UpdateCheckInCount();

-- -- Test code for NewCheckIn Trigger
-- SELECT businessID,
--     checkInCount
-- FROM Business
-- WHERE BusinessID = 'gnKjwL_1w79qoiV3IC_xQQ';
-- INSERT INTO ChecksIn
-- VALUES ('gnKjwL_1w79qoiV3IC_xQQ', '2021-03-23 12:00:00');
-- SELECT businessID,
--     checkInCount
-- FROM Business
-- WHERE BusinessID = 'gnKjwL_1w79qoiV3IC_xQQ';
-- -- clean 
-- DROP TRIGGER IF EXISTS NewCheckIn on ChecksIn;
-- DROP FUNCTION IF EXISTS UpdateCheckInCount;

-- DELETE FROM ChecksIn 
-- WHERE businessID = 'gnKjwL_1w79qoiV3IC_xQQ' AND checkInDate = '2021-03-23 12:00:00';

-- -- Test code for NewTip Trigger
-- SELECT businessID, tipCount
-- FROM Business
-- WHERE BusinessID = 'gnKjwL_1w79qoiV3IC_xQQ';
-- SELECT userID, tipCount, totalLikes
-- FROM Users
-- WHERE userID = 'jRyO2V1pA4CdVVqCIOPc1Q';
-- INSERT INTO Tip
-- VALUES ('jRyO2V1pA4CdVVqCIOPc1Q', 'gnKjwL_1w79qoiV3IC_xQQ', '2021-03-23 12:00:00', '10', 'Good Atmosphere!');
-- SELECT businessID, tipCount
-- FROM Business
-- WHERE BusinessID = 'gnKjwL_1w79qoiV3IC_xQQ';
-- SELECT userID, tipCount, totalLikes
-- FROM Users
-- WHERE userID = 'jRyO2V1pA4CdVVqCIOPc1Q';

-- -- clean 
-- DROP TRIGGER IF EXISTS NewTip on Tip;
-- DROP FUNCTION IF EXISTS UpdateTipInfo;

-- DELETE FROM Tip 
-- WHERE userID = 'jRyO2V1pA4CdVVqCIOPc1Q' AND businessID = 'gnKjwL_1w79qoiV3IC_xQQ' AND dateWritten = '2021-03-23 12:00:00';