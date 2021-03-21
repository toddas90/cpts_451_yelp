CREATE TABLE Users (
    userID VARCHAR,
    userName VARCHAR,
    averageStars REAL,  
    yelpingSince TIMESTAMP,
    tipCount INTEGER,
    totalLikes INTEGER,
    fans INTEGER,
    PRIMARY KEY (userID),
    CHECK (averageStars>=0 AND tipCount>=0 AND fans>=0)
);

CREATE TABLE UserLocation(
    userID VARCHAR,
    longitude FLOAT,
    latitude FLOAT,
    PRIMARY KEY (userID),
    FOREIGN KEY (userID) REFERENCES Users (userID)
);

CREATE TABLE UserRating(
    userID VARCHAR,
    funny INTEGER,
    cool INTEGER,
    useful INTEGER,
    PRIMARY KEY (userID),
    FOREIGN KEY (userID) REFERENCES Users (userID)
);

CREATE TABLE FriendsWith(
    userID VARCHAR,
    friendID VARCHAR,
    PRIMARY KEY (userID,friendID),
    FOREIGN KEY (userID) REFERENCES Users (userID),
    FOREIGN KEY (friendID) REFERENCES Users (userID)
);

CREATE TABLE Business(
    businessID VARCHAR,
    businessName VARCHAR,
    stars REAL,
    tipCount INTEGER,
    checkInCount INTEGER,
    isOpen BOOLEAN,
    PRIMARY KEY (businessID),
    CHECK (stars>=0 AND tipCount>=0)
);

CREATE TABLE ChecksIn(
    businessID VARCHAR,
    checkInDate TIMESTAMP,
    PRIMARY KEY (businessID,checkInDate),
    FOREIGN KEY (businessID) REFERENCES Business (businessID)
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
    latitude FLOAT,
    PRIMARY KEY (businessID),
    FOREIGN KEY (businessID) REFERENCES Business (businessID)
);

CREATE TABLE BusinessHours(
    businessID VARCHAR,
    dayOfWeek VARCHAR,
    openTime TIME,
    closeTime TIME,
    PRIMARY KEY (businessID, dayOfWeek),
    FOREIGN KEY (businessID) REFERENCES Business (businessID)
);

CREATE TABLE Attributes(
    businessID VARCHAR,
    attributeName VARCHAR,
    value VARCHAR,
    PRIMARY KEY (businessID, attributeName),
    FOREIGN KEY (businessID) REFERENCES Business (businessID)
);

CREATE TABLE Categories(
    businessID VARCHAR,
    categoryName VARCHAR,
    PRIMARY KEY (businessID, categoryName),
    FOREIGN KEY (businessID) REFERENCES Business (businessID)
);

CREATE TABLE Tip(
    userID VARCHAR, 
    businessID VARCHAR,
    dateWritten TIMESTAMP,
    likes INTEGER,
    textWritten VARCHAR,
    PRIMARY KEY (userID, businessID, datewritten),
    FOREIGN KEY (userID) REFERENCES Users (userID),
    FOREIGN KEY (businessID) REFERENCES Business (businessID)
);