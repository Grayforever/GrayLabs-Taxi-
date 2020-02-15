using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Taxi__.BroadcastReceivers
{
    public class CheckConnectivity : BroadcastReceiver
    {
        
        public override void OnReceive(Context context, Intent intent)
        {
            bool isConnected = intent.GetBooleanExtra(ConnectivityManager.ExtraNoConnectivity, false);
            if (isConnected)
            {
                Toast.MakeText(Application.Context, "internet connection lost", ToastLength.Long).Show();
            }
            else
            {
                Toast.MakeText(Application.Context, "internet connected", ToastLength.Long).Show();
            }
        }
    }
}