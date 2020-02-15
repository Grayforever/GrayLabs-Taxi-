using Firebase.Database;
using Java.Util;
using Taxi__.DataModels;
using Taxi__.Helpers;

namespace Taxi__.EventListeners
{
    public class RideRequestEventListener : Java.Lang.Object, IValueEventListener
    {
        NewTripDetails newTrip;
        FirebaseDatabase database;
        DatabaseReference newTripRef;
        SessionManager sessionManager = SessionManager.GetInstance();

        public void OnCancelled(DatabaseError error)
        {
            
        }
            
        public void OnDataChange(DataSnapshot snapshot)
        {
            
        }

        public RideRequestEventListener(NewTripDetails mNewTrip)
        {
            newTrip = mNewTrip;
            database = sessionManager.GetDatabase();
        }

        public async void CreateRequestAsync()
        {
            newTripRef = database.GetReference("Taxify_users").Child(sessionManager.GetCurrentUser()).Child("Ride_requests").Push();
            newTrip.RideID = newTripRef.Key;

            HashMap location = new HashMap();
            location.Put("latitude", newTrip.PickupLat);
            location.Put("longitude", newTrip.PickupLng);

            HashMap destination = new HashMap();
            destination.Put("latitude", newTrip.DestinationLat);
            destination.Put("longitude", newTrip.DestinationLng);

            //main hashmap
            HashMap myTrip = new HashMap();
            myTrip.Put("ride_id", newTrip.RideID);
            myTrip.Put("ride_status", newTrip.RideStatus);

            myTrip.Put("pickup_address", newTrip.PickupAddress);
            myTrip.Put("location", location);

            myTrip.Put("destination_address", newTrip.DestinationAddress);
            myTrip.Put("destination", destination);

            myTrip.Put("distance", newTrip.DistanceString);
            myTrip.Put("duration", newTrip.DurationString);
            myTrip.Put("ride_fare", newTrip.EstimateFare);

            myTrip.Put("payment_method", newTrip.Paymentmethod);
            myTrip.Put("created_at", newTrip.Timestamp.ToString());

            newTripRef.AddValueEventListener(this);
            await newTripRef.SetValueAsync(myTrip);
        }

        public async void CancelRequestAsync()
        {
            newTripRef.RemoveEventListener(this);
            await newTripRef.RemoveValueAsync();
        }

    }
}