TRUNCATE `housing3`;
LOAD DATA INFILE 'C:/xampp/htdocs/housing3_data.csv'
INTO TABLE `housing3`
FIELDS TERMINATED BY ','
ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 LINES
(
    longitude,
    latitude,
    housing_median_age,
    total_rooms,
    total_bedrooms,
    population,
    households,
    median_income,
    OCEAN,
    NEAR_OCEAN,
    NEAR_BAY,
    ISLAND,
    rooms_per_household,
    bedrooms_per_room,
    population_per_household,
    median_house_value,
    ocean_proximity
);