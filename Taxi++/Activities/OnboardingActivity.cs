using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Com.Mukesh.CountryPickerLib;
using Refractored.Controls;
using System;

namespace Taxi__.Activities
{
    [Activity(Label = "OnboardingActivity", Theme = "@style/AppTheme.MainScreen", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden, ScreenOrientation = ScreenOrientation.Portrait)]
    public class OnboardingActivity : AppCompatActivity
    {
        private RelativeLayout mRelativeLayout;
        private LinearLayout mLinearLayout;
        private LinearLayout ccLinear;
        private EditText mEditText;
        private CircleImageView countryFlagImg;

        public const int RequestCode = 100;
        public const int RequestPermission = 200;

        private CountryPicker.Builder builder;
        private CountryPicker picker;
        private Country country;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.onboarding_layout);
            GetWidgets();
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

        private void GetWidgets()
        {
            mRelativeLayout = (RelativeLayout)FindViewById(Resource.Id.onboard_root);

            mLinearLayout = (LinearLayout)FindViewById(Resource.Id.mLinear_view_2);
            ccLinear = (LinearLayout)FindViewById(Resource.Id.cc_layout_2);

            mEditText = (EditText)FindViewById(Resource.Id.user_phone_edittext2);
            mEditText.Click += MEditText_Click;
            mEditText.FocusChange += MEditText_FocusChange;
            mLinearLayout.Click += MLinearLayout_Click;

            //country code tools
            countryFlagImg = (CircleImageView)FindViewById(Resource.Id.country_flag_img_2);
            builder = new CountryPicker.Builder().With(this).SortBy(CountryPicker.SortByName);
            picker = builder.Build();
            country = picker.CountryFromSIM;
            countryFlagImg.SetBackgroundResource(country.Flag);
        }

        private void MEditText_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                GetSharedIntent();
            }
            
        }

        private void MEditText_Click(object sender, EventArgs e)
        {
            GetSharedIntent();
        }

        private void MLinearLayout_Click(object sender, EventArgs e)
        {
            GetSharedIntent();
        }

        private void GetSharedIntent()
        {
            var sharedIntent = new Intent(this, typeof(GetStartedActivity));
            Android.Support.V4.Util.Pair p1 = Android.Support.V4.Util.Pair.Create(ccLinear, "cc_trans");
            Android.Support.V4.Util.Pair p2 = Android.Support.V4.Util.Pair.Create(mEditText, "edittext_trans");

            ActivityOptionsCompat activityOptions = ActivityOptionsCompat.MakeSceneTransitionAnimation(this, p1, p2);
            StartActivity(sharedIntent, activityOptions.ToBundle());
        }

        protected override void OnResume()
        {
            base.OnResume();
            CheckLocationPermission();
        }
    }
}