import json

def cleanStr4SQL(s):
    return s.replace("'","`").replace("\n"," ")
    
def parseAttributes(d, outfile):
    for key in d.keys():
        outfile.write('(' + key + ',')
        if type(d[key]) == dict:
            parseAttributes(d[key], outfile)
        else:
            outfile.write(str(d[key]))
        
        outfile.write(') ')

def parseHours(days, outfile):
    for day in days.keys():
        outfile.write('(' + day + ',')
        open, close = days[day].split('-')
        outfile.write(open + ',' + close + ') ')

def parseBusinessData():
    #read the JSON file
    # We assume that the Yelp data files are available in the current directory. If not, you should specify the path when you "open" the function. 
    with open('yelp_business.JSON','r') as f:  
        outfile =  open('business.txt', 'w')
        line = f.readline()
        count_line = 0
        
        #read each JSON abject and extract data
        while line:
            data = json.loads(line)
            outfile.write(cleanStr4SQL(data['business_id'])+'\t') #business id
            outfile.write(cleanStr4SQL(data['name'])+'\t') #name
            outfile.write(cleanStr4SQL(data['address'])+'\t') #full_address
            outfile.write(cleanStr4SQL(data['state'])+'\t') #state
            outfile.write(cleanStr4SQL(data['city'])+'\t') #city
            outfile.write(cleanStr4SQL(data['postal_code']) + '\t')  #zipcode
            outfile.write(str(data['latitude'])+'\t') #latitude
            outfile.write(str(data['longitude'])+'\t') #longitude
            outfile.write(str(data['stars'])+'\t') #stars
            outfile.write(str(data['review_count'])+'\t') #reviewcount
            outfile.write(str(data['is_open'])+'\t') #openstatus

            categories = data["categories"].split(', ')
            outfile.write(str(categories)+'\t')  #category list
            
            parseAttributes(data['attributes'], outfile) #attributes
            
            outfile.write(';; ') #indicates end of attributes and start of hours
            
            parseHours(data['hours'], outfile) #hours

            outfile.write('\n');

            line = f.readline()
            count_line +=1
    print('parsed ' + str(count_line) + ' businesses')
    
    outfile.close()
    f.close()

def parseUserData():
    inFile = open('yelp_user.JSON', 'r')
    parsedFile = open('user.txt', 'w')
    
    line = inFile.readline()
    lineCount = 0
    
    while line:
        data = json.loads(line)
        parsedFile.write(data['user_id'] + ';') #user ID
        parsedFile.write(data['name'] + ';') #name
        parsedFile.write(data['yelping_since'].replace(' ', ';') + ';') #creation date;time
        parsedFile.write(str(data['average_stars']) + ';') #average stars
        parsedFile.write(str(data['tipcount']) + ';') #tip count
        parsedFile.write(str(data['cool']) + ';') #cool
        parsedFile.write(str(data['funny']) + ';') #funny
        parsedFile.write(str(data['useful']) + ';') #useful
        parsedFile.write(str(data['fans'])) #fans
        
        for friend in data['friends']:
            parsedFile.write(';' + friend) #friends
        
        parsedFile.write('\n')
        
        line = inFile.readline()
        
        lineCount += 1

    print('parsed ' + str(lineCount) + ' users')
    parsedFile.close()
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
    parseBusinessData()
    parseUserData()
    parseCheckinData()
    parseTipData()
