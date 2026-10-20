TRUNCATE `sales`;
LOAD DATA INFILE 'C:/xampp/htdocs/sales_data.csv'
INTO TABLE `housing`
FIELDS TERMINATED BY ',' 
ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 LINES
(部門,大分類,中分類,品種,年,月,売上);

TRUNCATE `units`;
LOAD DATA INFILE 'C:/xampp/htdocs/units_data.csv'
INTO TABLE `units`
FIELDS TERMINATED BY ',' 
ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 LINES
(品種,単位);

TRUNCATE `weather`;
LOAD DATA INFILE 'C:/xampp/htdocs/weather_data.csv'
INTO TABLE `weather`
FIELDS TERMINATED BY ',' 
ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 LINES
(年,月,平均気温(℃),最高気温(℃),最低気温(℃),降水量の合計(mm),日照時間(時間));
