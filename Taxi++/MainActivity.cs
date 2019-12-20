using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.Graphics.Drawable;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Google.Places;
using Java.Util;
using System;
using System.Collections.Generic;
using Taxi__.Activities;
using Taxi__.Adapters;
using Taxi__.DataModels;
using Taxi__.Fragments;
using Taxi__.Helpers;
using static Android.Content.IntentSender;
using static Android.Support.V4.View.ViewPager;

namespace Taxi__
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity, IOnMapReadyCallback, IOnPageChangeListener
    {
        #region variable declarations
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
        private TextView destinationText, txtMyLoc, txtDest;
        public ProgressBar progress;
        private Button mSelectRideBtn;

        //trip_bottomsheet
        Android.Support.V4.View.ViewPager pager;
        Android.Support.V4.View.PagerAdapter adapter;
        List<RideTypeDataModel> ride_type_list;

        //Map
        private GoogleMap mainMap;
        private SupportMapFragment mapFragment;
        private MapFunctionHelper mapHelper;
        private LocationRequest mLocationRequest;
        private FusedLocationProviderClient locationClient;
        private Location mLastLocation;
        private LocationCallbackHelper mLocationCallback;

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
        public string pickupAddress;

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

        #region Overrides
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.content_main);
            Instance = this;
            InitWidgets();
            InitMaps();
            CreateLocationRequestAsync();
            CreateHistory();
            GetHisstory();

            var api_key = GetString(Resource.String.map_key);

            if (!PlacesApi.IsInitialized)
            {
                PlacesApi.Initialize(this, api_key);
            }

            ride_type_list = new List<RideTypeDataModel>();
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

                            var json = await mapHelper.GetDirectionJsonAsync(myposition.Latitude, myposition.Longitude, destinationLatLng.Latitude, destinationLatLng.Longitude).ConfigureAwait(false);

                            if (!string.IsNullOrEmpty(json))
                            {
                                IsTripDrawn = true;
                                RunOnUiThread(() =>
                                {

                                    mapHelper.DrawTripOnMap(json);
                                    ride_type_list.Add(new RideTypeDataModel{ Image = Resource.Drawable.taxi, RidePrice = $"GH¢{mapHelper.EstimateFares().ToString()}", RideType = "Falaa"});
                                    ride_type_list.Add(new RideTypeDataModel { Image = Resource.Drawable.taxi, RidePrice = $"GH¢{mapHelper.EstimateFares().ToString()}", RideType = "Bossu" });

                                    adapter = new RidePagerAdapter(this, ride_type_list);
                                    pager = (Android.Support.V4.View.ViewPager)FindViewById(Resource.Id.viewPager1);
                                    pager.Adapter = adapter;
                                    pager.AddOnPageChangeListener(this);
                                    
                                    //txtFare.Text = $"GH¢{mapHelper.EstimateFares().ToString()}";
                                    //txtTime.Text = mapHelper.durationstring;
                                    txtDest.Text = place.Name;
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
                            RequestNewLocationData();
                            break;

                        case Result.Canceled:
                            behaviour.State = BottomSheetBehavior.StateHidden;
                            break;
                    }
                    break;
            }

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

        protected override void OnResume()
        {
            base.OnResume();
            CheckLocationPermission();
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

        #endregion

        #region custom methods
        private void InitWidgets()
        {
            var firstname = sessionManager.GetFirstname();
            mDrawer = (DrawerLayout)FindViewById(Resource.Id.drawer_layout);
            mToolbar = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.mapToolbar);
            SetSupportActionBar(mToolbar);
            SupportActionBar.Title = "";
            mToggle = new ActionBarDrawerToggle(this, mDrawer, mToolbar, Resource.String.open, Resource.String.close);
            mToggle.SyncState();
            mDrawer.AddDrawerListener(mToggle);

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
            //txtFare = (TextView)FindViewById(Resource.Id.trip_value_txt);
            //txtTime = (TextView)FindViewById(Resource.Id.trip_time_txt);
            mSelectRideBtn = (Button)FindViewById(Resource.Id.ride_select_btn);
            mSelectRideBtn.Click += MSelectRideBtn_Click;


            mainBottomSheet = (NestedScrollView)FindViewById(Resource.Id.main_bottom_root);
            behaviour = BottomSheetBehavior.From(mainBottomSheet);
            behaviour.PeekHeight = BottomSheetBehavior.PeekHeightAuto;
            behaviour.State = BottomSheetBehavior.StateHidden;

            fabMyLoc = (FloatingActionButton)FindViewById(Resource.Id.fab_myloc);
            fabMyLoc.Click += delegate
            {
                if (myposition != null && IsTripDrawn == false)
                {
                    mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(myposition, 15.0f));
                }
                else if(myposition == null && IsTripDrawn == false)
                {
                    GetDeviceLocation();
                }
            };

            //bottomsheet widgets
            name_tv = (TextView)FindViewById(Resource.Id.greetings_tv);
            dest_rl = (RelativeLayout)FindViewById(Resource.Id.layoutDestination);
            dest_rl.Click += Dest_rl_Click;

            name_tv.Text = GetGreetings(firstname);

            historyRecycler = (RecyclerView)FindViewById(Resource.Id.mRecycler);

        }

        public void ReverseTrip()
        {
            behaviour_trip.Hideable = false;
            behaviour_trip.State = BottomSheetBehavior.StateExpanded;
        }

        private void GetHisstory()
        {
            layoutManager = new Android.Support.V7.Widget.LinearLayoutManager(historyRecycler.Context);
            historyAdapter = new HistoryAdapter(recent_searches);
            historyRecycler.SetLayoutManager(layoutManager);
            historyRecycler.SetAdapter(historyAdapter);
        }

        private void CreateHistory()
        {
            recent_searches = new List<SearchHistoryModel>();
            if (string.IsNullOrEmpty(sessionManager.GetDestination()))
            {
                return;
            }
            recent_searches.Add(new SearchHistoryModel { PlaceName = sessionManager.GetDestination() });
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

        private string GetGreetings(string name)
        {
            string greeting = null;
            try
            {
                Date date = new Date();
                Calendar calendar = Calendar.Instance;
                calendar.Time = date;

                int hour = calendar.Get(CalendarField.HourOfDay);

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
            catch (Exception ex)
            {
                Console.WriteLine("Error", ex.Message);
            }

            return greeting;
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
                behaviour.State = BottomSheetBehavior.StateExpanded;

                RunOnUiThread(() =>
                {
                    mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(myposition, 15.0f));
                    mainMap.SetPadding(0, 0, 0, mainBottomSheet.Height);
                });

            }
        }

        private void SetTripUI()
        {
            behaviour.State = BottomSheetBehavior.StateExpanded;
            behaviour.Hideable = false;
            mainMap.SetPadding(0, 0, 0, mainBottomSheet.Height + 10);
            fabMyLoc.Visibility = ViewStates.Visible;
        }

        private void SaveToSharedPreference(string name, string destinationAddress, LatLng destinationLatLng)
        {
            editor = preferences.Edit();
            editor.PutString("place_name", name);
            editor.PutString("destinationAddress", destinationAddress);
            editor.PutString("destinationLatLng", destinationLatLng.ToString());

            editor.Apply();
        }

        #endregion

        #region events
        private void MSelectRideBtn_Click(object sender, EventArgs e)
        {
            behaviour.Hideable = true;
            behaviour.State = BottomSheetBehavior.StateHidden;
            behaviour_trip.Hideable = true;
            behaviour_trip.State = BottomSheetBehavior.StateHidden;
            FindingDriverDialog.Display(SupportFragmentManager, false);
        }

        private void HeaderView_Click(object sender, EventArgs e)
        {
            if (mDrawer.IsDrawerOpen((int)GravityFlags.Start))
            {
                ProfileFragment.Display(SupportFragmentManager, true);
                mDrawer.CloseDrawer((int)GravityFlags.Start);
            }
            return;
        }

        private void Dest_rl_Click(object sender, EventArgs e) => StartAutoComplete();

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
                    
                    break;

                case Resource.Id.action_history:
                    Places_History.Display(SupportFragmentManager, true);
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

        void MLocationCallback_MyLocation(object sender, LocationCallbackHelper.OnLocationCapturedEventArgs e)
        {
            mLastLocation = e.Location;
            LatLng mPosition = new LatLng(mLastLocation.Latitude, mLastLocation.Longitude);
            mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(mPosition, 15.0f));
            SetTripUI();

        }
        #endregion

        #region mapdata
        private void InitMaps()
        {
            mapFragment = (SupportMapFragment)SupportFragmentManager.FindFragmentById(Resource.Id.mapFragment);
            mapFragment.GetMapAsync(this);
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            mainMap = googleMap;
            InfoWindowHelper infoWindowHelper = new InfoWindowHelper(this);
            mainMap.SetInfoWindowAdapter(infoWindowHelper);

            mapHelper = new MapFunctionHelper(base.Resources.GetString(Resource.String.map_key), mainMap);
            UpdateLocationUi();
            GetDeviceLocation();
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

                fabMyLoc.Visibility = ViewStates.Gone;
                locationSettingsBuilder.SetAlwaysShow(false);
                LocationSettingsResult locationSettingsResult = await LocationServices.SettingsApi.CheckLocationSettingsAsync(
                    googleApiClient, locationSettingsBuilder.Build());

                var location_status = locationSettingsResult.Status.StatusCode;
                switch (location_status)
                {
                    case CommonStatusCodes.Success:
                        RunOnUiThread(() =>
                        {
                            SetTripUI();
                        });
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
                Org.Aviran.CookieBar2.CookieBar.Build(this)
               .SetTitle("Error")
               .SetMessage(ex.Message)
               .SetCookiePosition((int)GravityFlags.Bottom)
               .Show();
                
            }


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
                    mainMap.UiSettings.RotateGesturesEnabled = false;
                    mainMap.UiSettings.MapToolbarEnabled = false;
                }
                else
                {
                    mainMap.MyLocationEnabled = false;
                    mainMap.UiSettings.MyLocationButtonEnabled = false;
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
                if (CheckLocationPermission())
                {
                    mLastLocation = await locationClient.GetLastLocationAsync();
                    if (mLastLocation != null)
                    {
                        myposition = new LatLng(mLastLocation.Latitude, mLastLocation.Longitude);
                        mainMap.MoveCamera(CameraUpdateFactory.NewLatLngZoom(myposition, 15.0f));
                    }
                    else
                    {
                        RequestNewLocationData();
                    }
                }
                else
                {
                    CheckLocationPermission();
                }
                
            }
            catch
            {

            }
        }

        private void RequestNewLocationData()
        {
            mLocationRequest = LocationRequest.Create()
                .SetInterval(UPDATE_INTERVAL)
                .SetFastestInterval(FASTEST_INTERVAL)
                .SetPriority(LocationRequest.PriorityHighAccuracy)
                .SetNumUpdates(1);

            locationClient = LocationServices.GetFusedLocationProviderClient(this);

            mLocationCallback = new LocationCallbackHelper();
            mLocationCallback.MyLocation += MLocationCallback_MyLocation;

            locationClient.RequestLocationUpdates(mLocationRequest, mLocationCallback, null);
        }

        public void OnPageScrollStateChanged(int state)
        {
            
        }

        public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
        {
            
        }

        public void OnPageSelected(int position)
        {
            
        }

        #endregion

    }
}