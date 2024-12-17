namespace TestAssessment.Models;

public class TripData
{
    public DateTime PickupDatetime { get; set; }
    public DateTime DropoffDatetime { get; set; }
    public int? PassengerCount { get; set; }
    public double TripDistance { get; set; }
    public required string StoreAndFwdFlag { get; set; }
    public int PuLocationId { get; set; }
    public int DoLocationId { get; set; }
    public decimal FareAmount { get; set; }
    public decimal TipAmount { get; set; }
}