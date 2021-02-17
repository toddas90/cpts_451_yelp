import json

def cleanStr4SQL(s):
    return s.replace("'","`").replace("\n"," ")

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
            
            # TO-DO : write your own code to process attributes
            outfile.write(str([])) 
            # TO-DO : write your own code to process hours data
            outfile.write(str([])) 

            outfile.write('\n');

            line = f.readline()
            count_line +=1
    print(count_line)
    outfile.close()
    f.close()

def parseUserData():
    inFile = open('yelp_user.JSON', 'r')
    line = inFile.readline()
    data = json.loads(line)
    print(data)
    
    inFile.close()

def parseCheckinData():
    inFile = open('yelp_checkin.JSON', 'r')
    parsedFile = open('checkin.txt', 'w')
    line = inFile.readline()
    while line:
        data = json.loads(line)
        parsedFile.write(data['business_id'])
        parsedFile.write(';')
        checkins = data['date'].replace(' ', ';').replace(',',';')
        parsedFile.write(checkins)
        parsedFile.write('\n')
    
        line = inFile.readline()

    parsedFile.close()
    inFile.close()

def parseTipData():
    inFile = open('yelp_tip.JSON', 'r')
    parsedFile = open('tip.txt', 'w')
    line = inFile.readline()
    while line:
        data = json.loads(line)
        parsedFile.write(data['business_id'])
        parsedFile.write(';')
        parsedFile.write(data['user_id'])
        parsedFile.write(';')
        parsedFile.write(data['date'].replace(' ', ';'))
        parsedFile.write(';')
        parsedFile.write(str(data['likes']))
        parsedFile.write(';')
        parsedFile.write(data['text'])
        parsedFile.write('\n')
        
        line = inFile.readline()
    
    parsedFile.close()
    inFile.close()

if __name__ == "__main__":
    #parseBusinessData()
    parseUserData()
    #parseCheckinData()
    #parseTipData()
