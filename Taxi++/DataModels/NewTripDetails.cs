using System;

namespace Taxi__.DataModels
{
    public class NewTripDetails
    {
        public string RideID { get; set; }
        public string RideStatus { get; set; }

        public string PickupAddress { get; set; }
        public double PickupLat { get; set; }
        public double PickupLng { get; set; }

        public string DestinationAddress { get; set; }
        public double DestinationLat { get; set; }
        public double DestinationLng { get; set; }
        
        public string DistanceString { get; set; }
        public string DurationString { get; set; }
        
        public double EstimateFare { get; set; }
        public string Paymentmethod { get; set; }
        public DateTime Timestamp { get; set; }
    }
}