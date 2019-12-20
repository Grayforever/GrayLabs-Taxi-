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
    [Activity(Label = "@string/app_name", MainLauncher = true, Theme = "@style/AppTheme", NoHistory = false, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class SplashScreenActivity : AppCompatActivity
    {
        private SessionManager sessionManager;
        bool isConnected = CrossConnectivity.Current.IsConnected;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            sessionManager = SessionManager.GetInstance();
            // Create your application here
        }

        protected override void OnResume()
        {
            bool hasSession = sessionManager.GetSession();
            switch (isConnected)
            {
                case true:
                    switch (hasSession)
                    {
                        case true when !string.IsNullOrEmpty(sessionManager.GetFirstname()):
                            StartActivity(typeof(MainActivity));
                            Finish();
                            break;
                        default:
                            StartActivity(typeof(OnboardingActivity));
                            Finish();
                            break;
                    }

                    break;

                case false:
                    NoNetworkFragment.Display(SupportFragmentManager);
                    break;
            }
            base.OnResume();
        }
    }
}