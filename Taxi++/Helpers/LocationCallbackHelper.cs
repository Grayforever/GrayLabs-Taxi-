using System;
using Android.Gms.Location;
using Android.Util;

namespace Taxi__.Helpers
{
    public class LocationCallbackHelper : LocationCallback
    {
        public EventHandler<OnLocationCapturedEventArgs> MyLocation;

        public class OnLocationCapturedEventArgs : EventArgs
        {
            public Android.Locations.Location Location { get; set; }
        }

        public override void OnLocationAvailability(LocationAvailability locationAvailability)
        {
            Console.WriteLine("Mane Me", "IsLocationAvailable: {0}", locationAvailability.IsLocationAvailable);
        }

        public override void OnLocationResult(LocationResult result)
        {
           if (result.Locations.Count != 0)
           {
                MyLocation?.Invoke(this, new OnLocationCapturedEventArgs { Location = result.Locations[0] });
           }
        }
    }
}
