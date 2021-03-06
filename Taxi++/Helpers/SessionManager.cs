﻿using Android.App;
using Android.Content;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Java.Util.Concurrent;
using Taxi__.Activities;

namespace Taxi__.Helpers
{
    public class SessionManager
    {
        FirebaseApp app;
        FirebaseAuth auth;
        internal static SessionManager Instance { get; set; }

        static ISharedPreferences preferences = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);

        public static SessionManager GetInstance()
        {
            if (Instance == null)
                Instance = new SessionManager();
            return Instance;
        }

        public void InitializeFirebase()
        {
            if (app == null)
            {
                var options = new FirebaseOptions.Builder()

                    .SetApplicationId("taxiproject-185a4")
                    .SetApiKey("AIzaSyDHXqe3Yh9Nl3wsxFItOoz1IwKiBRP7fxk")
                    .SetDatabaseUrl("https://taxiproject-185a4.firebaseio.com")
                    .SetStorageBucket("taxiproject-185a4.appspot.com")
                    .Build();

                app = FirebaseApp.InitializeApp(Application.Context, options, "APP");
            }
        }

        public FirebaseAuth GetFirebaseAuth()
        {
            InitializeFirebase();
            if (auth == null)
                auth = FirebaseAuth.GetInstance(app);

            return auth;
        }

        public FirebaseDatabase GetDatabase()
        {
            var app = FirebaseApp.InitializeApp(Application.Context);
            FirebaseDatabase database;
            if (app == null)
            {
                var options = new FirebaseOptions.Builder()

                    .SetApplicationId("taxiproject-185a4")
                    .SetApiKey("AIzaSyDHXqe3Yh9Nl3wsxFItOoz1IwKiBRP7fxk")
                    .SetDatabaseUrl("https://taxiproject-185a4.firebaseio.com")
                    .SetStorageBucket("taxiproject-185a4.appspot.com")
                    .Build();

                app = FirebaseApp.InitializeApp(Application.Context, options);
                database = FirebaseDatabase.GetInstance(app);
            }
            else
            {
                database = FirebaseDatabase.GetInstance(app);
            }

            return database;
        }

        public FirebaseApp GetFirebaseAppInstance()
        {
            InitializeFirebase();
            return app;
        }

        public void SendVerificationCode(string numeroCelular, PhoneValidationActivity Instance)
        {
            InitializeFirebase();

            PhoneVerificationCallback phoneAuthCallbacks = new PhoneVerificationCallback(Instance);

            auth = GetFirebaseAuth();

            PhoneAuthProvider.GetInstance(auth).VerifyPhoneNumber(numeroCelular, 30, TimeUnit.Seconds, Instance, phoneAuthCallbacks);
        }

        public void SignOut()
        {
            InitializeFirebase();
            FirebaseAuth.GetInstance(app).SignOut();
        }

        public bool GetSession()
        {
            bool bResult = false;
            InitializeFirebase();

            var mAuth = GetFirebaseAuth();
            var user = mAuth.CurrentUser;
            if (user != null)
            {
                bResult = true;
            }
            return bResult;
        }

        public string GetCurrentUser()
        {
            InitializeFirebase();
            var mAuth = GetFirebaseAuth();
            var mUser = mAuth.CurrentUser.Uid;

            return mUser;
        }

        public string GetFirstname()
        {
            string firstname = preferences.GetString("firstname", "");
            return firstname;
        }

        public string GetLastName()
        {
            string lastname = preferences.GetString("lastname", "");
            return lastname;
        }

        public string GetEmail()
        {
            string email = preferences.GetString("email", "");
            return email;
        }

        public string GetPhone()
        {
            string phone = preferences.GetString("phone", "");
            return phone;
        }

        public string GetDestination()
        {
            string place = preferences.GetString("place_name", "");
            return place;
        }

        public string GetIntFormat()
        {
            string int_format = preferences.GetString("int_format", "");
            return int_format;
        }

        public string GetPhoneProto()
        {
            string phoneProto = preferences.GetString("phoneProto", "");
            return phoneProto;
        }

        public int GetLogintype()
        {
            int logintype = preferences.GetInt("logintype", 0);
            return logintype;
        }

        public string GetFbProfilePic()
        {
            string fbID = preferences.GetString("profile_id", "");
            return fbID;
        }
        
        public bool IsProviderLinked()
        {
            bool isLinked = preferences.GetBoolean("isLinked", false);

            return isLinked;
        }
    }
}