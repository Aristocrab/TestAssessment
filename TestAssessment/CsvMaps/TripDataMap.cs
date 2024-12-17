using CsvHelper.Configuration;
using TestAssessment.Models;

namespace TestAssessment.CsvMaps;

public sealed class TripDataMap : ClassMap<TripData>
{
    public TripDataMap()
    {
        Map(m => m.PickupDatetime).Name("tpep_pickup_datetime");
        Map(m => m.DropoffDatetime).Name("tpep_dropoff_datetime");
        Map(m => m.PassengerCount).Name("passenger_count");
        Map(m => m.TripDistance).Name("trip_distance");
        Map(m => m.StoreAndFwdFlag).Name("store_and_fwd_flag");
        Map(m => m.PuLocationId).Name("PULocationID");
        Map(m => m.DoLocationId).Name("DOLocationID");
        Map(m => m.FareAmount).Name("fare_amount");
        Map(m => m.TipAmount).Name("tip_amount");
    }
}