CREATE TABLE User (
    userID VARCHAR,
    userName VARCHAR,
    averageStars REAL,  
    yelpingSince DATETIME,
    tipCount INTEGER,
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
    FOREIGN KEY (userID,friendID) REFERENCES User (userID)
);

CREATE TABLE FansUser(
    fanID VARCHAR, --ID of user "fanning" other user
    userID VARCHAR, --ID of user being "fanned"
    PRIMARY KEY (fanID, userID),
    FOREIGN KEY (userID, fanID) REFERENCES User (userID)
);

CREATE TABLE LikesUser(
    likerID VARCHAR, --ID of user liking other user
    userID VARCHAR, --ID of user being liked
    reason VARCHAR, -- cool funny or useful
    PRIMARY KEY (likerID, userID),
    FOREIGN KEY (userID, likerID) REFERENCES User (userID)
);

CREATE TABLE ChecksIn(
    userID VARCHAR,
    businessID VARCHAR,
    checkInDate DATETIME,
    PRIMARY KEY (userID, businessID),
    FOREIGN KEY (userID) REFERENCES User (userID),
    FOREIGN KEY (businessID) REFERENCES Business (businessID)
);

CREATE TABLE business(
    businessID VARCHAR,
    businessName VARCHAR,
    stars REAL,
    tipCount INTEGER,
    isOpen BOOLEAN,
    wifi BOOLEAN,
    acceptsCreditCards BOOLEAN,
    goodForKids BOOLEAN,
    wheelchairAccessible BOOLEAN,
    dogsAllowed BOOLEAN,
    priceRange INTEGER, --1 through 4
    parking VARCHAR, --include bike parking
    catagories VARCHAR,
    PRIMARY KEY (businessID)
    CHECK (stars>=0, tipCount>=0)
);

CREATE TABLE Restaurant(
    businessID VARCHAR,
    attire VARCHAR,
    breakfast BOOLEAN,
    brunch BOOLEAN,
    lunch BOOLEAN,
    dinner BOOLEAN,
    dessert BOOLEAN,
    lateNight BOOLEAN,
    goodForGroups BOOLEAN,
    reservations BOOLEAN,
    outdoorSeating BOOLEAN, 
    hasTV BOOLEAN,
    noiseLevel VARCHAR,
    alcohol VARCHAR,
    catering BOOLEAN,
    delivery BOOLEAN,
    takeout BOOLEAN,
    PRIMARY KEY (businessID),
    FOREIGN KEY (businessID) REFERENCES Business (businessID)
);

CREATE TABLE RestaurantAmbiance(
    businessID VARCHAR,
    romantic BOOLEAN,
    intimate BOOLEAN,
    classy BOOLEAN,
    hipster BOOLEAN,
    divey BOOLEAN,
    touristy BOOLEAN,
    trendy BOOLEAN,
    upscale BOOLEAN,
    casual BOOLEAN,
    PRIMARY KEY (businessID),
    FOREIGN KEY (businessID) REFERENCES Restaurant (businessID)
);

-- includes salons, gyms, hotels, pharmacies, dry cleaners, hospitals/clinics, atms, banks, mail/shipping
CREATE TABLE PersonalService(
    businessID VARCHAR,
    appointmentOnly BOOLEAN,
    acceptsInsurance BOOLEAN,
    PRIMARY KEY (businessID),
    FOREIGN KEY (businessID) REFERENCES business (businessID)
);

-- parks, music, museums, attractions, libraries, movie theaters 
CREATE TABLE Entertainment(
    businessID VARCHAR,
    appointmentOnly BOOLEAN,
    PRIMARY KEY (businessID),
    FOREIGN KEY (businessID) REFERENCES Business (businessID)
);

CREATE TABLE BusinessAddress(
    businessID VARCHAR,
    businessState CHAR(2),
    businessCity VARCHAR,
    businessPostalCode CHAR(5),
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
    suHrs VARCHAR,
    mHrs VARCHAR,
    tHrs VARCHAR,
    wHrs VARCHAR,
    thHrs VARCHAR,
    fHrs VARCHAR,
    sHrs VARCHAR,
    PRIMARY KEY (businessID),
    FOREIGN KEY (businessID) REFERENCES Business (businessID)
);

CREATE TABLE Tip(
    userID VARCHAR, 
    businessID VARCHAR,
    dateWritten DATETIME,
    likes INTEGER,
    textWritten VARCHAR,
    PRIMARY KEY (userID, businessID),
    FOREIGN KEY (userID) REFERENCES User (userID),
    FOREIGN KEY (businessID) REFERENCES Business (businessID)
);