-- Update AND Trigger for up-to-date number of checkins in the Business table 
CREATE OR REPLACE FUNCTION UpdateNumCheckins() RETURNS trigger AS $$ 
BEGIN 
    IF (Business.businessID == NEW.businessID) THEN 
        NEW.CheckInCount = Business.CheckInCount + 1;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Update for up-to-date number of tips written about a business in Business table 
-- CREATE OR REPLACE FUNCTION UpdateNumTips() RETURNS trigger AS $$
-- BEGIN 
--     IF (Business.businessID == NEW.businessID) THEN 
--         NEW.NumTips = Business.NumTips + 1;
--     END IF;
--     RETURN NEW;
-- END;
-- $$ LANGUAGE plpgsql;

-- Update for up-to-date total number of likes in the Users table 
-- CREATE OR REPLACE FUNCTION UpdateTotalLikes() RETURNS trigger AS $$
-- BEGIN 
--     IF (Users.userID == NEW.userID) THEN 
--         NEW.totalLikes = Users.totalLikes + NEW.likes;
--     END IF;
--     RETURN NEW;
-- END;
-- $$ LANGUAGE plpgsql;

-- -- Update for up-to-date number of tips written by a user in the Users table 
-- CREATE OR REPLACE FUNCTION UpdateTipCount() RETURNS trigger AS $$
-- BEGIN 
--     IF (Users.userID == NEW.userID) THEN 
--         NEW.tipCount = Users.tipCount + 1;
--     END IF;
--     RETURN NEW;
-- END;
-- $$ LANGUAGE plpgsql;

-- Function combining UpdateNumTips, UpdateTotalLikes, and UpdateTipCount to be ran with the same trigger
CREATE OR REPLACE FUNCTION UpdateTipInfo() RETURNS trigger AS $$ 
BEGIN 
    IF (Users.userID == NEW.userID) THEN 
        NEW.tipCount = Users.tipCount + 1;
        NEW.totalLikes = Users.totalLikes + NEW.likes;
    END IF;
    IF (Business.businessID == NEW.businessID) THEN 
        NEW.NumTips = Business.NumTips + 1;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Trigger for updating tipCount in Business and Users, and totalLikes in Users
CREATE TRIGGER NewTip
AFTER
INSERT ON Tip FOR EACH STATEMENT EXECUTE FUNCTION UpdateTipInfo();

CREATE TRIGGER NewCheckIn
AFTER
INSERT ON ChecksIn FOR EACH STATEMENT EXECUTE FUNCTION UpdateNumCheckins();

-- Test code for NewCheckIn Trigger
SELECT businessID,
    checkInCount
FROM Business
WHERE BusinessID = 'gnKjwL_1w79qoiV3IC_xQQ';
INSERT INTO ChecksIn
VALUES ('gnKjwL_1w79qoiV3IC_xQQ', '2021-03-23 12:00:00');
SELECT businessID,
    checkInCount
FROM Business
WHERE BusinessID = 'gnKjwL_1w79qoiV3IC_xQQ';
-- clean 
DROP TRIGGER IF EXISTS NewCheckIn on ChecksIn;
-- Test code for NewTip Trigger
SELECT businessID,
    tipCount
FROM Business
WHERE BusinessID = 'gnKjwL_1w79qoiV3IC_xQQ';
SELECT userID,
    tipCount,
    totalLikes
FROM Users
WHERE userID = 'jRyO2V1pA4CdVVqCIOPc1Q';
INSERT INTO Tip
VALUES (
        'jRyO2V1pA4CdVVqCIOPc1Q',
        'gnKjwL_1w79qoiV3IC_xQQ',
        '2021-03-23 12:00:00',
        '10',
        'Good Atmosphere!'
    );
SELECT businessID,
    tipCount
FROM Business
WHERE BusinessID = 'gnKjwL_1w79qoiV3IC_xQQ';
SELECT userID,
    tipCount,
    totalLikes
FROM Users
WHERE userID = 'jRyO2V1pA4CdVVqCIOPc1Q';
-- clean 
-- DROP TRIGGER IF EXISTS NewTip on Business;
-- DROP TRIGGER IF EXISTS NewTip on Users;
DROP TRIGGER IF EXISTS NewTip on Tip;