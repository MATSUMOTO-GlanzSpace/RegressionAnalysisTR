TRUNCATE `housing`;
LOAD DATA INFILE 'C:/xampp/htdocs/housing_data.csv'
INTO TABLE `housing`
FIELDS TERMINATED BY ',' 
ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 LINES
(longitude, latitude, housing_median_age, total_rooms, total_bedrooms, population, households, median_income, OCEAN, NEAR_OCEAN, NEAR_BAY, ISLAND, median_house_value, ocean_proximity);