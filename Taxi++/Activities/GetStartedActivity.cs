using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Text;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using Com.Mukesh.CountryPickerLib;
using Com.Mukesh.CountryPickerLib.Listeners;
using Java.Lang;
using Plugin.Connectivity;
using Taxi__.Fragments;
using static Android.Views.View;
using Refractored.Controls;
using Android.Support.V4.Content;
using Android;
using Android.Util;

namespace Taxi__.Activities
{
    [Activity(Label = "GetStartedActivity", Theme = "@style/AppTheme.MainScreen", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden, ScreenOrientation = ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.AdjustResize)]
    public class GetStartedActivity : AppCompatActivity, ITextWatcher, IOnKeyListener, IOnCountryPickerListener
    {
        //widgets
        private Button PrimaryButton;
        private EditText UserPhoneText;
        private CircleImageView countryFlagImg;
        private LinearLayout CCLayout;
        private TextView CCTV;

        //dialogs
        private Android.Support.V7.App.AlertDialog alertDialog;
        private Android.Support.V7.App.AlertDialog.Builder alertBuilder;

        //country picker
        public static string phoneNumber;
        private CountryPicker.Builder builder;
        private CountryPicker picker;
        private Country country;

        internal static GetStartedActivity Instance { get; set; }

        public const int RequestCode = 100;
        public const int RequestPermission = 200;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.getting_started_layout);
            Instance = this;
            CheckLocationPermission();
           
            InitControls();
        }
       
        protected override void OnResume()
        {
            base.OnResume();
            CheckLocationPermission();
        }

        bool CheckLocationPermission()
        {
            bool permission_granted = false;

            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted && (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted))
            {
                //request permission
                permission_granted = false;
                RequestPermissions(new string[]
                {
                    Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation
                }, RequestPermission);
            }
            else
            {
                permission_granted = true;
            }
            return permission_granted;
        }

        public void AfterTextChanged(IEditable s)
        {

            ProcessButtonByTextLength();
        }

        private void ProcessButtonByTextLength()
        {
            var inputText = UserPhoneText.Text.ToString();
            if (inputText.Length >= 7)
            {
                PrimaryButton.Enabled = true;
            }
            else
            {
                PrimaryButton.Enabled = false;
            }
        }

        public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
        {
            
        }

        public bool OnKey(View v, [GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            var action = e.Action;
            if (action == KeyEventActions.Up)
            {
                ProcessButtonByTextLength();
            }
            return false;
        }

        public void OnTextChanged(ICharSequence s, int start, int before, int count)
        {
            
        }

        private void InitControls()
        {
            PrimaryButton = (Button)FindViewById(Resource.Id.primary_btn);
            PrimaryButton.Click += (s1, e1) =>
            {
                if (!CrossConnectivity.Current.IsConnected)
                {
                    Android.Support.V4.App.DialogFragment dialogFragment = new NoNetworkFragment();
                    dialogFragment.Show(SupportFragmentManager, "no network");
                    
                }
                else
                {
                    if (!ValidatePhoneNumberAndCode())
                    {
                        return;
                    }
                }
            };

            CCLayout = (LinearLayout)FindViewById(Resource.Id.cc_layout);
            CCLayout.Click += (s3, e3) =>
             {
                 picker.ShowBottomSheet(this);
             };
            CCTV = (TextView)FindViewById(Resource.Id.cc_textview);

            //country code tools
            countryFlagImg = (CircleImageView)FindViewById(Resource.Id.country_flag_img);
            countryFlagImg.RequestFocus();
            builder = new CountryPicker.Builder().With(this).Listener(this).SortBy(CountryPicker.SortByName);
            picker = builder.Build();
            country = picker.CountryFromSIM;
            countryFlagImg.SetBackgroundResource(country.Flag);
            CCTV.Text = country.DialCode;
            
            

            UserPhoneText = (EditText)FindViewById(Resource.Id.user_phone_edittext);
            UserPhoneText.AddTextChangedListener(this);
            UserPhoneText.SetOnKeyListener(this);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

           
        }

        private bool ValidatePhoneNumberAndCode()
        {
            ShowProgressDialog();
            phoneNumber = CCTV.Text + UserPhoneText.Text.Trim();
            if (Patterns.Phone.Matcher(phoneNumber).Matches())
            {
                try
                {

                    Intent myIntent = new Intent(this, typeof(PhoneValidationActivity));
                    myIntent.PutExtra("strPhoneNumber", phoneNumber);
                    StartActivity(myIntent);
                    OverridePendingTransition(Resource.Animation.slide_up_anim, Resource.Animation.slide_up_out);
                    CloseProgressDialog();
                }
                catch (System.Exception ex)
                {
                    Toast.MakeText(this, $"Error: {ex.Message}", ToastLength.Long).Show();
                }
            }
            else
            {
                CloseProgressDialog();
                Toast.MakeText(this, "Invalid phone number", ToastLength.Long).Show();
            }
           
            return true;
        }

        public void OnSelectCountry(Country country)
        {
            countryFlagImg.SetBackgroundResource(country.Flag);
            CCTV.Text = country.DialCode;
            UserPhoneText.RequestFocus();
        }

        public void ShowProgressDialog()
        {
            alertBuilder = new Android.Support.V7.App.AlertDialog.Builder(this);
            alertBuilder.SetView(Resource.Layout.progress_dialog_layout);
            alertBuilder.SetCancelable(false);
            alertDialog = alertBuilder.Show();
        }

        public void CloseProgressDialog()
        {
            if (alertDialog != null)
            {
                alertDialog.Dismiss();
                alertDialog = null;
                builder = null;
            }
        }

    }
}