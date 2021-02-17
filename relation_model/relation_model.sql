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
    buisnessID VARCHAR,
    checkInDate DATETIME,
    PRIMARY KEY (userID, buisnessID),
    FOREIGN KEY (userID) REFERENCES User (userID),
    FOREIGN KEY (buisnessID) REFERENCES Buisness (buisnessID)
);

CREATE TABLE Buisness(
    buisnessID VARCHAR,
    buisnessName VARCHAR,
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
    PRIMARY KEY (buisnessID)
    CHECK (stars>=0, tipCount>=0)
);

CREATE TABLE Restaurant(
    buisnessID VARCHAR,
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
    PRIMARY KEY (buisnessID),
    FOREIGN KEY (buisnessID) REFERENCES Buisness (buisnessID)
);

CREATE TABLE RestaurantAmbiance(
    buisnessID VARCHAR,
    romantic BOOLEAN,
    intimate BOOLEAN,
    classy BOOLEAN,
    hipster BOOLEAN,
    divey BOOLEAN,
    touristy BOOLEAN,
    trendy BOOLEAN,
    upscale BOOLEAN,
    casual BOOLEAN,
    PRIMARY KEY (buisnessID),
    FOREIGN KEY (buisnessID) REFERENCES Restaurant (buisnessID)
);

-- includes salons, gyms, hotels, pharmacies, dry cleaners, hospitals/clinics, atms, banks, mail/shipping
CREATE TABLE PersonalService(
    buisnessID VARCHAR,
    appointmentOnly BOOLEAN,
    acceptsInsurance BOOLEAN,
    PRIMARY KEY (buisnessID),
    FOREIGN KEY (buisnessID) REFERENCES Buisness (buisnessID)
);

-- parks, music, museums, attractions, libraries, movie theaters 
CREATE TABLE Entertainment(
    buisnessID VARCHAR,
    appointmentOnly BOOLEAN,
    PRIMARY KEY (buisnessID),
    FOREIGN KEY (buisnessID) REFERENCES Buisness (buisnessID)
);

CREATE TABLE BuisnessAddress(
    buisnessID VARCHAR,
    buisnessState CHAR(2),
    buisnessCity VARCHAR,
    buisnessPostalCode CHAR(5),
    PRIMARY KEY (buisnessID),
    FOREIGN KEY (buisnessID) REFERENCES Buisness (buisnessID)
);


CREATE TABLE BuisnessLocation(
    buisnessID VARCHAR,
    longitude FLOAT,
    lattitude FLOAT,
    PRIMARY KEY (buisnessID),
    FOREIGN KEY (buisnessID) REFERENCES Buisness (buisnessID)
);

CREATE TABLE BuisnessHours(
    buisnessID VARCHAR,
    suHrs VARCHAR,
    mHrs VARCHAR,
    tHrs VARCHAR,
    wHrs VARCHAR,
    thHrs VARCHAR,
    fHrs VARCHAR,
    sHrs VARCHAR,
    PRIMARY KEY (buisnessID),
    FOREIGN KEY (buisnessID) REFERENCES Buisness (buisnessID)
);

CREATE TABLE Tip(
    userID VARCHAR, 
    buisnessID VARCHAR,
    dateWritten DATETIME,
    likes INTEGER,
    textWritten VARCHAR,
    PRIMARY KEY (userID, buisnessID),
    FOREIGN KEY (userID) REFERENCES User (userID),
    FOREIGN KEY (buisnessID) REFERENCES Buisness (buisnessID)
);