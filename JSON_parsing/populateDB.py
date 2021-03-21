import json
import psycopg2
from tqdm import tqdm

def cleanStr4SQL(s):
    return s.replace("'","`").replace("\n"," ")

# inserts nested attributes in the form (businessID, parentName:name, value)
def insertNestedBusinessAttributes(id, attributes, parent, cursor, db):
    for key in attributes.keys():
        if type(attributes[key]) == dict:
            if not insertNestedBusinessAttribute(id, attributes[key], parent + ':' + key, cursor, db):
                return False
        else:
            try:
                cursor.execute("INSERT INTO Attributes (businessID, attributeName, value)"
                              + " VALUES (%s, %s, %s)",
                              (id, parent + ':' + key, attributes[key]) )
            except Exception as e:
                print("unable to insert into Attributes", e)
                return False
            db.commit()

    return True
    
def insertBusinessAttributes(id, attributes, cursor, db):
    for key in attributes.keys():
        if type(attributes[key]) == dict:
            if not insertNestedBusinessAttributes(id, attributes[key], key, cursor, db):
                return False
        else:
            try:
                cursor.execute("INSERT INTO Attributes (businessID, attributeName, value)"
                              + " VALUES (%s, %s, %s)",
                              (id, key, attributes[key]) )
            except Exception as e:
                print("unable to insert into Attributes", e)
                return False
            db.commit()

    return True

def insertBusinessHours(id, hours, cursor, db):
    for day in hours.keys():
        open, close = hours[day].split('-')

        try:
            cursor.execute("INSERT INTO BusinessHours (businessID, dayOfWeek, openTime, closeTime)"
                          + " VALUES (%s, %s, %s, %s)", 
                          (id, day, open, close) )              
        except Exception as e:
            print("unable to insert into BusinessHours", e)
            return
        db.commit()

    return True

def insertBusinessCategories(id, categories, cursor, db):
    for category in categories:
        try:
            cursor.execute("INSERT INTO Categories (businessID, categoryName)"
                          + " VALUES (%s, %s)", 
                          (id, category) )              
        except Exception as e:
            print("unable to insert into Categories", e)
            return False
        db.commit()

    return True

def insertBusinessData(cursor, db):
    inFile = open('yelp_business.JSON', 'r')
    
    line = inFile.readline()
    lineCount = 0
    
    # count file length (for progress bar)
    while line:
        lineCount += 1
        line = inFile.readline()
    
    inFile.seek(0)    # reset to beginning of file
    
    for i in tqdm(range(lineCount), desc='populating business info'):
        line = inFile.readline()
        data = json.loads(line)
        
        # business data
        try:
            cursor.execute("INSERT INTO Business (businessID, businessName, stars, tipCount, checkInCount, isOpen)"
                          + " VALUES (%s, %s, %s, %s, %s, %s)", 
                          (data['business_id'], data['name'], data['stars'], 0, 0, bool(data['is_open'])) )              
        except Exception as e:
            print("unable to insert into Business", e)
            return
        db.commit()
    
        # business address
        try:
            cursor.execute("INSERT INTO BusinessAddress (businessID, businessState, businessCity, businessPostalCode, businessStreetAddress)"
                          + " VALUES (%s, %s, %s, %s, %s)", 
                          (data['business_id'], data['state'], data['city'], data['postal_code'], data['address']) )
        except Exception as e:
            print("unable to insert into BusinessAddress", e)
            return
        db.commit()
    
        # business location
        try:
            cursor.execute("INSERT INTO BusinessLocation (businessID, longitude, latitude)"
                          + " VALUES (%s, %s, %s)", 
                          (data['business_id'], data['longitude'], data['latitude']) )              
        except Exception as e:
            print("unable to insert into BusinessLocation", e)
            return
        db.commit()
        
        if not insertBusinessHours(data['business_id'], data['hours'], cursor, db):
            return
        
        if not insertBusinessCategories(data['business_id'], data['categories'].split(', '), cursor, db):
            return
            
        if not insertBusinessAttributes(data['business_id'], data['attributes'], cursor, db):
            return

    inFile.close()

#inserts user data to users table
def insertUserData(cursor, db):
    inFile = open('yelp_user.JSON', 'r')
    
    line = inFile.readline()
    lineCount = 0
    
    # count file length (for progress bar)
    while line:
        lineCount += 1
        line = inFile.readline()
    
    inFile.seek(0)    # reset to beginning of file
    
    for i in tqdm(range(lineCount), desc='populating user info'):
        line = inFile.readline()
        data = json.loads(line)

        # user data
        try:
            cursor.execute("INSERT INTO Users (userID, userName, averageStars, yelpingSince, tipCount, totalLikes, fans)"
                          + " VALUES (%s, %s, %s, %s, %s, %s, %s)", 
                          (data['user_id'], data['name'], data['average_stars'], data['yelping_since'], data['tipcount'], 0, data['fans']) )              
        except Exception as e:
            print("unable to insert into Users", e)
            return
        db.commit()
        
        # user location
        try:
            cursor.execute("INSERT INTO UserLocation (userID, longitude, latitude)"
                          + " VALUES (%s, %s, %s)",
                          (data['user_id'], 0, 0) )
        except Exception as e:
            print("unable to insert into UserLocation", e)
            return
        db.commit()
        
        # user rating
        try:
            cursor.execute("INSERT INTO UserRating (userID, funny, cool, useful)"
                          + " VALUES (%s, %s, %s, %s)",
                          (data['user_id'], data['funny'], data['cool'], data['useful']) )
        except Exception as e:
            print("unable to insert into UserRating", e)
            return
        db.commit()

    inFile.seek(0)    # reset to beginning
    
    # add friends info, must be done after users due to foreign key constraints
    for i in tqdm(range(lineCount), desc='populating friend info'):
        line = inFile.readline()
        data = json.loads(line)
        
        for friend in data['friends']:
            try:
                cursor.execute("INSERT INTO FriendsWith (userID, friendID)"
                              + " VALUES (%s, %s)",
                              (data['user_id'], friend) )
            except Exception as e:
                print("unable to insert into FriendsWith", e)
                return
            db.commit()

    inFile.close()

def parseCheckinData():
    inFile = open('yelp_checkin.JSON', 'r')
    parsedFile = open('checkin.txt', 'w')
    line = inFile.readline()
    lineCount = 0
    while line:
        data = json.loads(line)
        parsedFile.write(data['business_id']) #business ID
        parsedFile.write(';')
        checkins = data['date'].replace(' ', ';').replace(',',';')
        parsedFile.write(checkins) #date;time;date;time...
        parsedFile.write('\n')
    
        line = inFile.readline()
        lineCount += 1

    print('parsed ' + str(lineCount) + ' checkins')
    parsedFile.close()
    inFile.close()

def parseTipData():
    inFile = open('yelp_tip.JSON', 'r')
    parsedFile = open('tip.txt', 'w')
    line = inFile.readline()
    lineCount = 0
    while line:
        data = json.loads(line)
        parsedFile.write(data['business_id']) #business ID
        parsedFile.write(';')
        parsedFile.write(data['user_id']) #user ID
        parsedFile.write(';')
        parsedFile.write(data['date'].replace(' ', ';')) #date;time
        parsedFile.write(';')
        parsedFile.write(str(data['likes'])) #likes
        parsedFile.write(';')
        parsedFile.write(data['text']) #description
        parsedFile.write('\n')
        
        line = inFile.readline()
        lineCount += 1
    
    print('parsed ' + str(lineCount) + ' tips')
    parsedFile.close()
    inFile.close()

if __name__ == "__main__":
    dbConnection = None
    
    error = False
    
    try:
        dbConnection = psycopg2.connect("dbname='yelpdata' user='postgres' host='localhost' password=''")   # change parameters as needed
    except Exception as e:
        error = True
        print('fatal error occured', e)
    
    if not error:
        dbCursor = dbConnection.cursor()
    
        insertUserData(dbCursor, dbConnection)
        insertBusinessData(dbCursor, dbConnection)
    
        dbCursor.close()
        dbConnection.close()
