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
using Android.Support.V4.View;
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
using Taxi__.Adapters;
using Taxi__.DataModels;
using Taxi__.EventListeners;
using Taxi__.Fragments;
using Taxi__.Helpers;
using static Android.Content.IntentSender;
using static Android.Support.Design.Widget.NavigationView;
using static Android.Support.V4.View.ViewPager;
using Taxi__.Constants;
using FFImageLoading;
using Refractored.Controls;
using Taxi__.Utils;

namespace Taxi__
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity, IOnMapReadyCallback, IOnPageChangeListener, IOnNavigationItemSelectedListener
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
        private RelativeLayout mainBottomSheet;
        private TextView destinationText, txtMyLoc, txtDest;
        public ProgressBar progress;
        private Button mSelectRideBtn;

        //trip details
        private NewTripDetails trip_details;
        public RideRequestEventListener RideRequestEvent;

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
        private static readonly long UPDATE_INTERVAL = 5 * 1000;
        private static readonly long FASTEST_INTERVAL = UPDATE_INTERVAL / 2;
        private static readonly long MAX_WAIT_TIME = UPDATE_INTERVAL * 2;
        private static readonly float SMALLEST_DISPLACEMENT = 1.0f;
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

            var api_key = GetString(Resource.String.map_key);

            if (!PlacesApi.IsInitialized)
            {
                PlacesApi.Initialize(this, api_key);
            }

            ride_type_list = new List<RideTypeDataModel>();
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

            switch (requestCode)
            {
                case REQUEST_CODE_PLACE:
                    switch (resultCode)
                    {
                        case Result.Ok:
                            var place = Autocomplete.GetPlaceFromIntent(data);

                            
                            progress.Visibility = ViewStates.Visible;

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
                                    ride_type_list.Clear();
                                    mapHelper.DrawTripOnMap(json);
                                    ride_type_list.Add(new RideTypeDataModel{ Image = Resource.Drawable.taxi, RidePrice = $"GH¢{mapHelper.EstimateFares().ToString()}", RideType = "Falaa"});
                                    ride_type_list.Add(new RideTypeDataModel { Image = Resource.Drawable.taxi, RidePrice = $"GH¢{mapHelper.EstimateFares().ToString()}", RideType = "Bossu" });

                                    adapter = new RidePagerAdapter(this, ride_type_list);
                                    pager.Adapter = adapter;
                                    pager.AddOnPageChangeListener(this);
                                    
                                    txtDest.Text = place.Name;
                                    txtMyLoc.Text = mapHelper.GetStartAddress();

                                    behaviour.Hideable = true;
                                    behaviour.State = BottomSheetBehavior.StateHidden;

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
            
        }

        protected override void OnPause()
        {
            base.OnPause();
            ImageService.Instance.SetExitTasksEarly(true);
        }

        protected override void OnResume()
        {
            base.OnResume();
            CheckLocationPermission();
            ImageService.Instance.SetExitTasksEarly(false);
        }

        public override void OnTrimMemory([GeneratedEnum] TrimMemory level)
        {
            ImageService.Instance.InvalidateMemoryCache();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            base.OnTrimMemory(level);
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
            var fbId = sessionManager.GetFbProfilePic();

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
            
            drawerTextUsername = (TextView)headerView.FindViewById(Resource.Id.accountTitle);
            drawerTextUsername.Text = firstname;

            var accountImage = (CircleImageView)headerView.FindViewById(Resource.Id.accountImage);

            RunOnUiThread(() =>
            {
                SetProfilePic(fbId, accountImage);
            });
            
            SetUpDrawerContent(navView);

            //bottomsheet
            trip_details_bottomsheet = (RelativeLayout)FindViewById(Resource.Id.trip_root);
            behaviour_trip = BottomSheetBehavior.From(trip_details_bottomsheet);
            if (IsTripDrawn == false)
            {
                behaviour_trip.State = BottomSheetBehavior.StateHidden;
            }

            pager = (Android.Support.V4.View.ViewPager)FindViewById(Resource.Id.viewPager1);
            txtMyLoc = (TextView)FindViewById(Resource.Id.from_tv1);
            txtDest = (TextView)FindViewById(Resource.Id.to_tv1);

            mSelectRideBtn = (Button)FindViewById(Resource.Id.ride_select_btn);
            mSelectRideBtn.Click += MSelectRideBtn_Click;


            mainBottomSheet = (RelativeLayout)FindViewById(Resource.Id.main_sheet_root);
            behaviour = BottomSheetBehavior.From(mainBottomSheet);
            behaviour.PeekHeight = BottomSheetBehavior.PeekHeightAuto;
            behaviour.State = BottomSheetBehavior.StateHidden;

            fabMyLoc = (FloatingActionButton)FindViewById(Resource.Id.fab_myloc);
            fabMyLoc.Click += delegate
            {
                if (myposition != null && IsTripDrawn == false)
                {
                    
                    mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(myposition, 17.0f));    
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

            destinationText = (TextView)FindViewById(Resource.Id.destinationText);
            progress = (ProgressBar)FindViewById(Resource.Id.progress1);
        }

        private void SetUpDrawerContent(NavigationView navView)
        {
            navView.SetNavigationItemSelectedListener(this);
        }

        public bool OnNavigationItemSelected(IMenuItem menuItem)
        {
            SelectDrawerItem(menuItem.ItemId);
            return true;
        }

        private void SelectDrawerItem(int itemId)
        {
            Android.Support.V4.App.Fragment fragment = null;

            switch (itemId)
            {
                case Resource.Id.action_free_rides:
                    break;

                case Resource.Id.action_payments:
                    fragment = new PaymentsFragment();
                    break;

                case Resource.Id.action_history:
                    fragment = new Places_History();
                    break;

                case Resource.Id.action_promos:

                    break;

                case Resource.Id.action_support:

                    break;

                case Resource.Id.action_about:

                    break;

            }

            if (fragment != null)
            {
                SupportFragmentManager
                    .BeginTransaction()
                    .SetCustomAnimations(Resource.Animation.slide_up_anim, Resource.Animation.slide_up_out)
                    .Replace(Resource.Id.content_frame, fragment)
                    .AddToBackStack(null)
                    .Commit();
            }
            mDrawer.CloseDrawer(GravityCompat.Start);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            mToggle.OnOptionsItemSelected(item);
            return base.OnOptionsItemSelected(item);
        }

        private async void SetProfilePic(string providerID, CircleImageView accountImage)
        {
            try
            {
                await ImageService.Instance
                .LoadUrl($"https://graph.facebook.com/{providerID}/picture?type=normal")
                .LoadingPlaceholder("boy_new", FFImageLoading.Work.ImageSource.CompiledResource)
                .Retry(3, 200)
                .IntoAsync(accountImage);
            }
            catch(Exception ex)
            {

            }
            
        }

        

        public void ReverseTrip()
        {
            behaviour_trip.Hideable = false;
            behaviour_trip.State = BottomSheetBehavior.StateExpanded;
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

            if (IsTripDrawn != true)
            {
                return;
            }
            dest_rl.Enabled = true;
            progress.Visibility = ViewStates.Gone;
            name_tv.Text = $"Welcome back, {firstname}";
            destinationText.Text = "Search destination";
            IsTripDrawn = false;
            mainMap.Clear();
            behaviour_trip.Hideable = true;
            behaviour_trip.State = BottomSheetBehavior.StateHidden;
            behaviour.State = BottomSheetBehavior.StateExpanded;
            behaviour.Hideable = false;

            RunOnUiThread(() =>
            {
                mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(myposition, 17.0f));
                mainMap.SetPadding(0, 0, 0, mainBottomSheet.Height);
            });
        }

        private void SetTripUI()
        {
            behaviour.State = BottomSheetBehavior.StateExpanded;
            behaviour.Hideable = false;
            //mainMap.SetPadding(0, 0, 0, mainBottomSheet.Height + 10);
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

        public void LockDrawer()
        {
            mDrawer.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);
        }
        
        public void UnlockDrawer()
        {
            mDrawer.SetDrawerLockMode(DrawerLayout.LockModeUnlocked);
        }
        #endregion

        #region events
        private void MSelectRideBtn_Click(object sender, EventArgs e)
        {
            behaviour.Hideable = true;
            behaviour.State = BottomSheetBehavior.StateHidden;
            behaviour_trip.Hideable = true;
            behaviour_trip.State = BottomSheetBehavior.StateHidden;

            FindingDriverDialog.Display(SupportFragmentManager, false, mapHelper.GetStartAddress(), destinationAddress, mapHelper.durationstring, mapHelper.EstimateFares());
            trip_details = new NewTripDetails
            {
                RideStatus = "awaiting driver",
                PickupAddress = mapHelper.GetStartAddress(),
                PickupLat = myposition.Latitude,
                PickupLng = myposition.Longitude,

                DestinationAddress = destinationAddress,
                DestinationLat = destinationLatLng.Latitude,
                DestinationLng = destinationLatLng.Longitude,

                DistanceString = mapHelper.distanceString,
                DurationString = mapHelper.durationstring,

                EstimateFare = mapHelper.EstimateFares(),
                Paymentmethod = "cash",
                Timestamp = DateTime.UtcNow
            };

            RideRequestEvent = new RideRequestEventListener(trip_details);
            RideRequestEvent.CreateRequestAsync();
        }

        private void HeaderView_Click(object sender, EventArgs e)
        {
            if (mDrawer.IsDrawerOpen((int)GravityFlags.Start))
            {
                Android.Support.V4.App.Fragment profileFragment = new ProfileFragment();
                SupportFragmentManager
                    .BeginTransaction()
                    .SetCustomAnimations(Resource.Animation.slide_up_anim, Resource.Animation.slide_up_out)
                    .Replace(Resource.Id.content_frame, profileFragment, profileFragment.Class.SimpleName)
                    .AddToBackStack(null)
                    .Commit();
                mDrawer.CloseDrawer((int)GravityFlags.Start);
            }
            return;
        }

        private void Dest_rl_Click(object sender, EventArgs e) => StartAutoComplete();

        private void MLocationCallback_MyLocation(object sender, LocationCallbackHelper.OnLocationCapturedEventArgs e)
        {
            mLastLocation = e.Location;
            LatLng mPosition = new LatLng(mLastLocation.Latitude, mLastLocation.Longitude);
            mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(mPosition, 17.0f));
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
                    .SetPriority(LocationRequest.PriorityHighAccuracy)
                    .SetSmallestDisplacement(SMALLEST_DISPLACEMENT)
                    .SetMaxWaitTime(MAX_WAIT_TIME);

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
                        mainMap.MoveCamera(CameraUpdateFactory.NewLatLngZoom(myposition, 17.0f));
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