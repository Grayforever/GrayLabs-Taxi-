using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Google.Places;
using System;
using System.Collections.Generic;
using Taxi__.Activities;
using Taxi__.Adapters;
using Taxi__.DataModels;
using Taxi__.Helpers;
using static Android.Content.IntentSender;

namespace Taxi__
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity, IOnMapReadyCallback
    {
        #region variables
        //widgets
        private Android.Support.V7.Widget.Toolbar mToolbar;
        private DrawerLayout mDrawer;
        private ActionBarDrawerToggle mToggle;
        private NavigationView navView;
        private TextView drawerTextUsername, name_tv;
        public BottomSheetBehavior behaviour;
        public BottomSheetBehavior behaviour_trip;
        private FloatingActionButton fabMyLoc;
        private RelativeLayout dest_rl, trip_details_bottomsheet;
        private NestedScrollView mainBottomSheet;
        private TextView txtFare, txtTime, destinationText, txtMyLoc, txtDest;
        public ProgressBar progress;

        //Map
        private GoogleMap mainMap;
        private SupportMapFragment mapFragment;
        private MapFunctionHelper mapHelper;
        private LocationRequest mLocationRequest;
        private FusedLocationProviderClient locationClient;
        private Location mLastLocation;

        //constants
        private const int REQUEST_CODE_PLACE = 0;
        private const int REQUEST_CODE_LOCATION = 1;
        private static readonly int UPDATE_INTERVAL = 10000;
        private static readonly int FASTEST_INTERVAL = UPDATE_INTERVAL / 2;
        private bool IsTripDrawn = false;

        //TripDetails
        private LatLng destinationLatLng;
        private LatLng myposition;
        private string destinationAddress;

        private readonly string[] permissionGroupLocation = { Manifest.Permission.AccessFineLocation, Manifest.Permission.AccessCoarseLocation };
        private const int requestLocationId = 0;

        //Menu
        private IMenuItem previousItem;

        //offline preferences
        readonly SessionManager sessionManager = SessionManager.GetInstance();
        internal static MainActivity Instance { get; set; }
        //shared preference
        ISharedPreferences preferences = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);
        ISharedPreferencesEditor editor;

        //Recent Search
        List<SearchHistoryModel> recent_searches;
        HistoryAdapter historyAdapter;
        RecyclerView.LayoutManager layoutManager;
        RecyclerView historyRecycler;

        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.content_main);
            Instance = this;
            InitWidgets();
            InitMaps();

            CreateHistory();
            GetHisstory();

            var api_key = GetString(Resource.String.map_key);

            if (!PlacesApi.IsInitialized)
            {
                PlacesApi.Initialize(this, api_key);
            }

        }

        private void InitMaps()
        {
            mapFragment = (SupportMapFragment)SupportFragmentManager.FindFragmentById(Resource.Id.mapFragment);
            mapFragment.GetMapAsync(this);
        }

        private void InitWidgets()
        {
            var firstname = sessionManager.GetFirstname();
            mDrawer = (DrawerLayout)FindViewById(Resource.Id.drawer_layout);
            mToolbar = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.mapToolbar);
            SetSupportActionBar(mToolbar);
            SupportActionBar.Title = "";
            mToggle = new ActionBarDrawerToggle(this, mDrawer, mToolbar, Resource.String.open, Resource.String.close);
            mDrawer.AddDrawerListener(mToggle);
            mToolbar.SetNavigationIcon(Resource.Drawable.ic_nav_menu);

            //NavigationView
            navView = (NavigationView)FindViewById(Resource.Id.navView);
            navView.ItemIconTintList = null;

            var headerView = navView.GetHeaderView(0);
            headerView.Click += HeaderView_Click;
            navView.NavigationItemSelected += NavView_NavigationItemSelected;
            drawerTextUsername = (TextView)headerView.FindViewById(Resource.Id.accountTitle);
            drawerTextUsername.Text = firstname;

            //bottomsheet
            trip_details_bottomsheet = (RelativeLayout)FindViewById(Resource.Id.trip_root);
            behaviour_trip = BottomSheetBehavior.From(trip_details_bottomsheet);
            if (IsTripDrawn == false)
            {
                behaviour_trip.State = BottomSheetBehavior.StateHidden;
            }

            txtMyLoc = (TextView)FindViewById(Resource.Id.from_tv1);
            txtDest = (TextView)FindViewById(Resource.Id.to_tv1);

            //mSelectRideBtn = (Button)FindViewById(Resource.Id.ride_select_btn);


            mainBottomSheet = (NestedScrollView)FindViewById(Resource.Id.main_bottom_root);
            behaviour = BottomSheetBehavior.From(mainBottomSheet);
            behaviour.PeekHeight = BottomSheetBehavior.PeekHeightAuto;
            behaviour.State = BottomSheetBehavior.StateHidden;

            fabMyLoc = (FloatingActionButton)FindViewById(Resource.Id.fab_myloc);
            fabMyLoc.Click += delegate
            {
                if (myposition == null)
                {
                    return;
                }
                else
                {
                    mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(myposition, 15.0f));
                }
                
            };

            //bottomsheet widgets
            name_tv = (TextView)FindViewById(Resource.Id.greetings_tv);
            dest_rl = (RelativeLayout)FindViewById(Resource.Id.layoutDestination);
            dest_rl.Click += Dest_rl_Click;

            name_tv.Text = GetGreetings(firstname);

            historyRecycler = (RecyclerView)FindViewById(Resource.Id.mRecycler);

        }

        void GetHisstory()
        {
            layoutManager = new Android.Support.V7.Widget.LinearLayoutManager(historyRecycler.Context);
            historyRecycler.SetLayoutManager(layoutManager);
            historyRecycler.SetAdapter(historyAdapter);
        }

        void CreateHistory()
        {
            recent_searches = new List<SearchHistoryModel>();
            recent_searches.Add(new SearchHistoryModel { PlaceName = sessionManager.GetDestination() });
        }

        private void HeaderView_Click(object sender, EventArgs e)
        {
            if (mDrawer.IsDrawerOpen((int)GravityFlags.Start))
            {
                mDrawer.CloseDrawer((int)GravityFlags.Start);
                StartActivity(typeof(MainProfileActivity));
                OverridePendingTransition(Resource.Animation.slide_up_anim, Resource.Animation.slide_up_out);
            }
            return;
        }

        private string GetGreetings(string name)
        {
            string greeting = null;
            try
            {
                Java.Util.Date date = new Java.Util.Date();
                Java.Util.Calendar calendar = Java.Util.Calendar.Instance;
                calendar.Time = date;

                int hour = calendar.Get(Java.Util.CalendarField.HourOfDay);

                if (hour >= 12 && hour < 17)
                {
                    greeting = $"Good Afternoon, {name}";
                }
                else if (hour >= 17 && hour < 21)
                {
                    greeting = $"Good Evening, {name}";
                }
                else if (hour >= 21 && hour < 24)
                {
                    greeting = $"Good Night, {name}";
                }
                else
                {
                    greeting = $"Good Morning, {name}";
                }
                
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error", ex.Message);
            }

            return greeting;
        }

        private void StartAutoComplete()
        {
            List<Place.Field> fields = new List<Place.Field>
            {
                Place.Field.Id,
                Place.Field.Name,
                Place.Field.LatLng,
                Place.Field.Address
            };

            Intent intent = new Autocomplete.IntentBuilder(AutocompleteActivityMode.Fullscreen, fields)
                .SetCountry("GH")
                .Build(this);

            StartActivityForResult(intent, REQUEST_CODE_PLACE);
        }

        private void Dest_rl_Click(object sender, EventArgs e) => StartAutoComplete();

        private async void CreateLocationRequestAsync()
        {
            try
            {
                GoogleApiClient googleApiClient = new GoogleApiClient.Builder(this)
                    .AddApi(LocationServices.API)
                    .Build();
                googleApiClient.Connect();

                mLocationRequest = LocationRequest.Create()
                    .SetInterval(UPDATE_INTERVAL)
                    .SetFastestInterval(FASTEST_INTERVAL)
                    .SetPriority(LocationRequest.PriorityHighAccuracy);

                locationClient = LocationServices.GetFusedLocationProviderClient(this);
                

                LocationSettingsRequest.Builder locationSettingsBuilder = new LocationSettingsRequest.Builder()
                    .AddLocationRequest(mLocationRequest);

                locationSettingsBuilder.SetAlwaysShow(false);
                LocationSettingsResult locationSettingsResult = await LocationServices.SettingsApi.CheckLocationSettingsAsync(
                    googleApiClient, locationSettingsBuilder.Build());

                var location_status = locationSettingsResult.Status.StatusCode;
                switch (location_status)
                {
                    case CommonStatusCodes.Success:
                        GetDeviceLocation();
                        behaviour.State = BottomSheetBehavior.StateExpanded;
                        mainMap.SetPadding(0, 0, 0, mainBottomSheet.Height);
                        break;

                    case CommonStatusCodes.ResolutionRequired:
                        try
                        {
                            locationSettingsResult.Status.StartResolutionForResult(this, REQUEST_CODE_LOCATION);
                        }
                        catch (SendIntentException e)
                        {
                            _ = Log.Debug("sendIntentException", e.Message);
                        }
                        break;

                    case LocationSettingsStatusCodes.SettingsChangeUnavailable:
                        //
                        break;
                }
                
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Error: {ex.Message}", ToastLength.Short).Show();
            }


        }

        private void SaveToSharedPreference(string name, string destinationAddress, LatLng destinationLatLng)
        {
            editor = preferences.Edit();
            editor.PutString("place_name", name);
            editor.PutString("destinationAddress", destinationAddress);
            editor.PutString("destinationLatLng", destinationLatLng.ToString());

            editor.Apply();
        }

        private void NavView_NavigationItemSelected(object sender, NavigationView.NavigationItemSelectedEventArgs e)
        {
            if (previousItem != null)
                previousItem.SetChecked(false);

            navView.SetCheckedItem(e.MenuItem.ItemId);
            previousItem = e.MenuItem;

            switch (e.MenuItem.ItemId)
            {
                case Resource.Id.action_free_rides:
                    break;

                case Resource.Id.action_payments:
                    StartActivity(typeof(PaymentsActiviity));
                    OverridePendingTransition(Resource.Animation.slide_up_anim, Resource.Animation.slide_up_out);
                    break;

                case Resource.Id.action_history:
                    break;

                case Resource.Id.action_promos:
                    break;

                case Resource.Id.action_support:
                    break;

                case Resource.Id.action_about:
                    break;


            }
            mDrawer.CloseDrawers();
        }

        private bool CheckLocationPermission()
        {
            bool permissionGranted;
            if (Android.Support.V4.Content.ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted &&
                Android.Support.V4.Content.ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted)
            {
                permissionGranted = false;
                RequestPermissions(permissionGroupLocation, requestLocationId);
            }
            else
            {
                permissionGranted = true;
            }

            return permissionGranted;
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            mainMap = googleMap;
            InfoWindowHelper infoWindowHelper = new InfoWindowHelper(this);
            mainMap.SetInfoWindowAdapter(infoWindowHelper);

            mapHelper = new MapFunctionHelper(base.Resources.GetString(Resource.String.map_key), mainMap);
            UpdateLocationUi();
            CreateLocationRequestAsync();
            
        }

        private void UpdateLocationUi()
        {
            if (mainMap == null)
            {
                return;
            }

            try
            {
                if (CheckLocationPermission())
                {
                    mainMap.MyLocationEnabled = true;
                    mainMap.UiSettings.MyLocationButtonEnabled = false;
                    mainMap.UiSettings.CompassEnabled = false;
                }
                else
                {
                    mainMap.MyLocationEnabled = false;
                    mainMap.UiSettings.MyLocationButtonEnabled = false;
                    mLastLocation = null;
                    CheckLocationPermission();
                }
            }
            catch (Java.Lang.SecurityException)
            {

            }
        }

        private async void GetDeviceLocation()
        {
            try
            {
                if (!CheckLocationPermission())
                {
                    return;
                }
                mLastLocation = await locationClient.GetLastLocationAsync();
                if (mLastLocation != null)
                {
                    myposition = new LatLng(mLastLocation.Latitude, mLastLocation.Longitude);
                    mainMap.MoveCamera(CameraUpdateFactory.NewLatLngZoom(myposition, 15.0f));
                }
                else
                {
                    return;
                }
            }
            catch
            {

            }
        }

        private void ResetTrip()
        {
            var firstname = sessionManager.GetFirstname();

            if (IsTripDrawn == true)
            {
                dest_rl.Enabled = true;
                progress.Visibility = ViewStates.Gone;
                name_tv.Text = $"Welcome back, {firstname}";
                destinationText.Text = "Search destination";
                IsTripDrawn = false;
                mainMap.Clear();
                behaviour_trip.Hideable = true;
                behaviour_trip.State = BottomSheetBehavior.StateHidden;

                RunOnUiThread(() =>
                {
                    mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(myposition, 15.0f));
                    mainMap.SetPadding(0, 0, 0, mainBottomSheet.Height);
                });
                
            }
        }

        public override void OnBackPressed()
        {
            if (mDrawer.IsDrawerOpen((int)GravityFlags.Start))
            {
                mDrawer.CloseDrawer((int)GravityFlags.Start);
            }
            else
            {
                switch (IsTripDrawn)
                {
                    case true:
                        ResetTrip();
                        break;
                    default:
                        base.OnBackPressed();
                        break;
                }
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (mToggle.OnOptionsItemSelected(item))
            {
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            try
            {
                if (grantResults.Length > 1)
                {
                    return;
                }
                if (grantResults[0] == (int)Permission.Granted)
                {
                    GetDeviceLocation();
                }
            }
            catch (IndexOutOfRangeException iore)
            {
                Console.WriteLine(iore.Message);
            }
        }

        protected async override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            destinationText = (TextView)FindViewById(Resource.Id.destinationText);
            progress = (ProgressBar)FindViewById(Resource.Id.progress1);
            progress.Visibility = ViewStates.Visible;

            switch (requestCode)
            {
                case REQUEST_CODE_PLACE:
                    switch (resultCode)
                    {
                        case Result.Ok:
                            var place = Autocomplete.GetPlaceFromIntent(data);

                            name_tv.Text = "Please wait...";
                            destinationText.Text = place.Name;
                            destinationAddress = place.Address;
                            destinationLatLng = place.LatLng;
                            dest_rl.Enabled = false;
                            SaveToSharedPreference(place.Name, destinationAddress, destinationLatLng);

                            
                            var json = await mapHelper.GetDirectionJsonAsync(myposition, destinationLatLng);
                            var myloc = await mapHelper.FindCordinateAddress(myposition);
                            if (!string.IsNullOrEmpty(json) || !string.IsNullOrEmpty(myloc))
                            {
                                IsTripDrawn = true;
                                txtFare = (TextView)FindViewById(Resource.Id.trip_value_txt);
                                txtTime = (TextView)FindViewById(Resource.Id.trip_time_txt);
                                RunOnUiThread(() =>
                                {

                                    mapHelper.DrawTripOnMap(json);
                                    txtFare.Text = $"GH¢{mapHelper.EstimateFares().ToString()}";
                                    txtTime.Text = mapHelper.durationstring;
                                    txtDest.Text = place.Name;
                                    txtMyLoc.Text = myloc;
                                    behaviour_trip.State = BottomSheetBehavior.StateExpanded;
                                    behaviour_trip.Hideable = false;
                                    mainMap.SetPadding(0, 0, 0, trip_details_bottomsheet.Height + 10);

                                });
                            }
                            else
                            {
                                return;
                            }

                            break;

                        case Result.Canceled:
                            progress.Visibility = ViewStates.Gone;
                            break;
                    }
                    break;

                case REQUEST_CODE_LOCATION:
                    switch (resultCode)
                    {
                        case Result.Ok:
                            behaviour.State = BottomSheetBehavior.StateExpanded;
                            GetDeviceLocation();
                            break;

                        case Result.Canceled:
                            behaviour.State = BottomSheetBehavior.StateHidden;
                            break;
                    }
                    break;
            }

        }

        protected override void OnResume()
        {
            base.OnResume();
            CheckLocationPermission();
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnStop()
        {
            base.OnStop();
            //StopLocationUpdatesAsync();
        }

        protected override void OnPause()
        {
            base.OnPause();
        }
    }
}