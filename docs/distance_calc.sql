-- returns the distance between the two points in miles
CREATE OR REPLACE FUNCTION getDistance(lat1 FLOAT, lon1 FLOAT, lat2 FLOAT, lon2 FLOAT)
RETURNS FLOAT AS $$
DECLARE
	lat1Rad FLOAT = RADIANS(lat1);
	lon1Rad FLOAT = RADIANS(lon1);
	lat2Rad FLOAT = RADIANS(lat2);
	lon2Rad FLOAT = RADIANS(lon2);

	earthRadius FLOAT = 3958.8;  -- radius of the earth in miles

	haversine FLOAT = POWER(SIN((lat2Rad - lat1Rad) / 2), 2) + (COS(lat1Rad) * COS(lat2Rad) * POWER(SIN((lon2Rad - lon1Rad) / 2), 2)); -- haversine formula

	distance FLOAT = 2 * earthRadius * ASIN(SQRT(haversine));  -- distance in miles

BEGIN
	RETURN distance;
END;
$$ LANGUAGE plpgsql;