import json
import psycopg2
from tqdm import tqdm

def cleanStr4SQL(s):
    return s.replace("'","`").replace("\n"," ")

# inserts nested attributes in the form (businessID, parentName:name, value)
def insertNestedBusinessAttributes(id, attributes, parent, cursor):
    for key in attributes.keys():
        if type(attributes[key]) == dict:
            if not insertNestedBusinessAttribute(id, attributes[key], parent + ':' + key, cursor):
                return False
        elif type(attributes[key]) == str and (attributes[key] == 'False' or attributes[key] == 'no' or attributes[key] == 'none'):
            continue
        else:
            try:
                cursor.execute("INSERT INTO Attributes (businessID, attributeName, value)"
                              + " VALUES (%s, %s, %s)",
                              (id, parent + ':' + key, attributes[key]) )
            except Exception as e:
                print("unable to insert into Attributes", e)
                return False

    return True
    
def insertBusinessAttributes(id, attributes, cursor):
    for key in attributes.keys():
        if type(attributes[key]) == dict:
            if not insertNestedBusinessAttributes(id, attributes[key], key, cursor):
                return False
        elif type(attributes[key]) == str and (attributes[key] == 'False' or attributes[key] == 'no' or attributes[key] == 'none'):
            continue
        else:
            try:
                cursor.execute("INSERT INTO Attributes (businessID, attributeName, value)"
                              + " VALUES (%s, %s, %s)",
                              (id, key, attributes[key]) )
            except Exception as e:
                print("unable to insert into Attributes", e)
                return False

    return True

def insertBusinessHours(id, hours, cursor):
    for day in hours.keys():
        open, close = hours[day].split('-')

        try:
            cursor.execute("INSERT INTO BusinessHours (businessID, dayOfWeek, openTime, closeTime)"
                          + " VALUES (%s, %s, %s, %s)", 
                          (id, day, open, close) )              
        except Exception as e:
            print("unable to insert into BusinessHours", e)
            return

    return True

def insertBusinessCategories(id, categories, cursor):
    for category in categories:
        try:
            cursor.execute("INSERT INTO Categories (businessID, categoryName)"
                          + " VALUES (%s, %s)", 
                          (id, category) )              
        except Exception as e:
            print("unable to insert into Categories", e)
            return False

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
    
        # business address
        try:
            cursor.execute("INSERT INTO BusinessAddress (businessID, businessState, businessCity, businessPostalCode, businessStreetAddress)"
                          + " VALUES (%s, %s, %s, %s, %s)", 
                          (data['business_id'], data['state'], data['city'], data['postal_code'], data['address']) )
        except Exception as e:
            print("unable to insert into BusinessAddress", e)
            return
        
    
        # business location
        try:
            cursor.execute("INSERT INTO BusinessLocation (businessID, longitude, latitude)"
                          + " VALUES (%s, %s, %s)", 
                          (data['business_id'], data['longitude'], data['latitude']) )              
        except Exception as e:
            print("unable to insert into BusinessLocation", e)
            return
        
        if not insertBusinessHours(data['business_id'], data['hours'], cursor):
            return
        
        if not insertBusinessCategories(data['business_id'], data['categories'].split(', '), cursor):
            return
            
        if not insertBusinessAttributes(data['business_id'], data['attributes'], cursor):
            return

    db.commit()

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
                          (data['user_id'], data['name'], data['average_stars'], data['yelping_since'], 0, 0, data['fans']) )
        except Exception as e:
            print("unable to insert into Users", e)
            return

        # user location
        try:
            cursor.execute("INSERT INTO UserLocation (userID, longitude, latitude)"
                          + " VALUES (%s, %s, %s)",
                          (data['user_id'], 0, 0) )
        except Exception as e:
            print("unable to insert into UserLocation", e)
            return

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

def insertCheckinData(cursor, db):
    inFile = open('yelp_checkin.JSON', 'r')
    
    line = inFile.readline()
    lineCount = 0
    
    # count file length
    while line:
        lineCount += 1
        line = inFile.readline()
    
    inFile.seek(0)    # reset to beginning of file
    
    for i in tqdm(range(lineCount), desc='populating checkin info'):
        line = inFile.readline()
        data = json.loads(line)
        
        checkinDates = data['date'].split(',')
        
        for checkin in checkinDates:
            try:
                cursor.execute("INSERT INTO ChecksIn (businessID, checkInDate)"
                              + " VALUES (%s, %s)",
                              (data['business_id'], checkin) )
            except Exception as e:
                print("unable to insert into ChecksIn", e)
                return

    db.commit()

    inFile.close()

def insertTipData(cursor, db):
    inFile = open('yelp_tip.JSON', 'r')

    line = inFile.readline()
    lineCount = 0
    
    # count file length
    while line:
        lineCount += 1
        line = inFile.readline()
    
    inFile.seek(0)    # reset to beginning of file

    for i in tqdm(range(lineCount), desc='populating tip info'):
        line = inFile.readline()
        data = json.loads(line)
        
        try:
            cursor.execute("INSERT INTO Tip (userID, businessID, dateWritten, likes, textWritten)"
                          + " VALUES (%s, %s, %s, %s, %s)",
                          (data['user_id'], data['business_id'], data['date'], data['likes'], data['text']) )
        except Exception as e:
            print("unable to insert into Tip", e)
            return

    db.commit()

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
        insertCheckinData(dbCursor, dbConnection)
        insertTipData(dbCursor, dbConnection)
    
        dbCursor.close()
        dbConnection.close()
