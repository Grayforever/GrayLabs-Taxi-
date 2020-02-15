using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase.Database;
using Taxi__.DataModels;
using Taxi__.Helpers;

namespace Taxi__.EventListeners
{
    public class RecentHistoryListener : Java.Lang.Object, IValueEventListener
    {
        List<NewTripDetails> recentTripList = new List<NewTripDetails>();

        SessionManager sessionManager = SessionManager.GetInstance();

        public event EventHandler<RecentTripEventArgs> HistoryRetrieved;
        public class RecentTripEventArgs : EventArgs
        {
            public List<NewTripDetails> RecentTripList { get; set; }
        }

        public void OnCancelled(DatabaseError error)
        {
            
        }

        public void OnDataChange(DataSnapshot snapshot)
        {
            if(snapshot.Value != null)
            {
                var child = snapshot.Children.ToEnumerable<DataSnapshot>();
                recentTripList.Clear();

                foreach(DataSnapshot searchData in child)
                {
                    NewTripDetails tripDetails = new NewTripDetails();
                    tripDetails.RideID = searchData.Key;
                    tripDetails.PickupLat = (double)searchData.Child("location").Child("latitude").Value;
                    tripDetails.PickupLng = (double)searchData.Child("location").Child("longitude").Value;
                    tripDetails.DestinationLat = (double)searchData.Child("destination").Child("latitude").Value;
                    tripDetails.DestinationLng = (double)searchData.Child("destination").Child("longitude").Value;
                    tripDetails.DestinationAddress = searchData.Child("destination_address").Value.ToString();

                    recentTripList.Add(tripDetails);
                }
                HistoryRetrieved.Invoke(this, new RecentTripEventArgs { RecentTripList = recentTripList });
            }
        }

        public void Create()
        {
            var dbRef = sessionManager.GetDatabase().GetReference("Taxify_users").Child(sessionManager.GetCurrentUser()).Child("Ride_requests");
            dbRef.AddValueEventListener(this);
        }
    }
}