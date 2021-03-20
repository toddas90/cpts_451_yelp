CREATE TABLE User (
    userID VARCHAR,
    userName VARCHAR,
    averageStars REAL,  
    yelpingSince DATETIME,
    tipCount INTEGER,
    totalLikes INTEGER,
    fans INTEGER,
    PRIMARY KEY (userID),
    CHECK (averageStars>=0, tipCount>=0, fans>=0)
);

CREATE TABLE UserLocation(
    userID VARCHAR,
    longitude FLOAT,
    lattitude FLOAT,
    PRIMARY KEY (userID),
    FOREIGN KEY (userID) REFERENCES User (userID)
);

CREATE TABLE UserRating(
    userID VARCHAR,
    funny INTEGER,
    cool INTEGER,
    useful INTEGER,
    PRIMARY KEY (userID),
    FOREIGN KEY (userID) REFERENCES User (userID)
);

CREATE TABLE FriendsWith(
    userID VARCHAR,
    friendID VARCHAR,
    PRIMARY KEY (userID,friendID),
    FOREIGN KEY (userID) REFERENCES User (userID),
    FOREIGN KEY (friendID) REFERENCES User (userID)
);

CREATE TABLE ChecksIn(
    businessID VARCHAR,
    checkInDate DATETIME,
    PRIMARY KEY (businessID,checkInDate),
    FOREIGN KEY (businessID) REFERENCES Business (businessID)
);

CREATE TABLE Business(
    businessID VARCHAR,
    businessName VARCHAR,
    stars REAL,
    tipCount INTEGER,
    checkInCount INTEGER,
    isOpen BOOLEAN,
    PRIMARY KEY (businessID)
    CHECK (stars>=0, tipCount>=0)
);


CREATE TABLE BusinessAddress(
    businessID VARCHAR,
    businessState CHAR(2),
    businessCity VARCHAR,
    businessPostalCode CHAR(5),
    businessStreetAddress VARCHAR,
    PRIMARY KEY (businessID),
    FOREIGN KEY (businessID) REFERENCES Business (businessID)
);


CREATE TABLE BusinessLocation(
    businessID VARCHAR,
    longitude FLOAT,
    lattitude FLOAT,
    PRIMARY KEY (businessID),
    FOREIGN KEY (businessID) REFERENCES Business (businessID)
);

CREATE TABLE BusinessHours(
    businessID VARCHAR,
    dayOfWeek VARCHAR,
    openTime VARCHAR,
    closeTime VARCHAR
    PRIMARY KEY (businessID,),
    FOREIGN KEY (businessID) REFERENCES Business (businessID)
);

CREATE TABLE Attributes(
    businessID VARCHAR,
    attributeName VARCHAR,
    value BOOLEAN,
    PRIMARY KEY (buisnessID, attributeName),
    FOREIGN KEY (businessID) REFERENCES Business (businessID)
);

CREATE TABLE Categories(
    businessID VARCHAR,
    categoryName VARCHAR,
    PRIMARY KEY (businessID, categoryName)
    FOREIGN KEY (businessID) REFERENCES Business (businessID)
);

CREATE TABLE Tip(
    userID VARCHAR, 
    businessID VARCHAR,
    dateWritten DATETIME,
    likes INTEGER,
    textWritten VARCHAR,
    PRIMARY KEY (userID, businessID, datewritten),
    FOREIGN KEY (userID) REFERENCES User (userID),
    FOREIGN KEY (businessID) REFERENCES Business (businessID)
);