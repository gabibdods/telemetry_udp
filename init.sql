CREATE TABLE IF NOT EXISTS telemetry (
    id INT AUTO_INCREMENT PRIMARY KEY,
    speed SMALLINT UNSIGNED,
    throttle FLOAT,
    steer FLOAT,
    brake FLOAT,
    clutch TINYINT UNSIGNED,
    gear TINYINT,
    engineRPM SMALLINT UNSIGNED,
    drs TINYINT UNSIGNED,
    revLightsPercent TINYINT UNSIGNED,
    revLightsBitValue SMALLINT UNSIGNED,
    brakesTemperature0 SMALLINT UNSIGNED,
    brakesTemperature1 SMALLINT UNSIGNED,
    brakesTemperature2 SMALLINT UNSIGNED,
    brakesTemperature3 SMALLINT UNSIGNED,
    tyresSurfaceTemperature0 TINYINT UNSIGNED,
    tyresSurfaceTemperature1 TINYINT UNSIGNED,
    tyresSurfaceTemperature2 TINYINT UNSIGNED,
    tyresSurfaceTemperature3 TINYINT UNSIGNED,
    tyresInnerTemperature0 TINYINT UNSIGNED,
    tyresInnerTemperature1 TINYINT UNSIGNED,
    tyresInnerTemperature2 TINYINT UNSIGNED,
    tyresInnerTemperature3 TINYINT UNSIGNED,
    engineTemperature SMALLINT UNSIGNED,
    tyresPressure0 FLOAT,
    tyresPressure1 FLOAT,
    tyresPressure2 FLOAT,
    tyresPressure3 FLOAT,
    surfaceType0 TINYINT UNSIGNED,
    surfaceType1 TINYINT UNSIGNED,
    surfaceType2 TINYINT UNSIGNED,
    surfaceType3 TINYINT UNSIGNED,
    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Ensure efficient queries on latest timestamp
DROP INDEX idx_timestamp_desc ON telemetry;
CREATE INDEX idx_timestamp_desc ON telemetry (timestamp DESC);