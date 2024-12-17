CREATE TABLE TripData (
    Id INT IDENTITY PRIMARY KEY,
    PickupDatetime DATETIME2 NOT NULL,
    DropoffDatetime DATETIME2 NOT NULL,
    PassengerCount INT NOT NULL,
    TripDistance FLOAT NOT NULL,
    StoreAndFwdFlag VARCHAR(3) NOT NULL,
    PuLocationId INT NOT NULL,
    DoLocationId INT NOT NULL,
    FareAmount DECIMAL(10, 2) NOT NULL,
    TipAmount DECIMAL(10, 2) NOT NULL
);

CREATE INDEX IX_PULocationId ON TripData (PuLocationId);
CREATE INDEX IX_TipAmount ON TripData (PuLocationId, TipAmount DESC);
CREATE INDEX IX_TripDistance ON TripData (TripDistance DESC);
CREATE INDEX IX_TripDuration ON TripData (PickupDatetime, DropoffDatetime);