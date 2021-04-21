-- returns the distance between the two points in miles
CREATE FUNCTION getDistance(@lat1 FLOAT, @lon1 FLOAT, @lat2 FLOAT, @lon2 FLOAT)
RETURNS FLOAT
AS
BEGIN
	DECLARE @lat1Rad FLOAT = RADIANS(lat1);
	DECLARE @lon1Rad FLOAT = RADIANS(lon1);
	DECLARE @lat2Rad FLOAT = RADIANS(lat2);
	DECLARE @lon2Rad FLOAT = RADIANS(lon2);

	DECLARE @earthRadius FLOAT = 3958.8;  -- radius of the earth in miles

	DECLARE @haversine FLOAT = SQUARE(SIN((@lat2Rad - lat1Rad) / 2)) + (COS(@lat1Rad) * COS(@lat2Rad) * SQUARE(SIN((@lon2Rad - @lon1Rad) / 2))); -- haversine formula

	DECLARE @distance FLOAT = 2 * @earthRadius * ASIN(SQRT(@haversine));  -- distance in miles
	
	RETURN @distance;
END;