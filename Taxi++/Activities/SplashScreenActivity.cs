using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
//using Firebase.Auth;
using Plugin.Connectivity;
using Taxi__.Fragments;
using Taxi__.Helpers;

namespace Taxi__.Activities
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Theme = "@style/AppTheme.Splash", NoHistory = false, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class SplashScreenActivity : AppCompatActivity
    {
        private SessionManager sessionManager;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
        }

        protected override void OnResume()
        {
            
            if (!CrossConnectivity.Current.IsConnected)
            {
                Android.Support.V4.App.DialogFragment dialogFragment = new NoNetworkFragment();
                dialogFragment.Show(SupportFragmentManager, "no network");
            }
            else
            {
                sessionManager = SessionManager.GetInstance();
                string firstname = sessionManager.GetFirstname();
                if (!string.IsNullOrEmpty(firstname))
                {
                    StartActivity(typeof(MainActivity));
                    Finish();
                }
                else
                {
                    StartActivity(typeof(GetStartedActivity));
                    Finish();
                }
            }
            base.OnResume();
        }
    }
}