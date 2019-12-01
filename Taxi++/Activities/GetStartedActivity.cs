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
using Android.Graphics;
using Google.I18n.PhoneNumbers;
using Taxi__.Helpers;
using Android.Views.InputMethods;

namespace Taxi__.Activities
{
    [Activity(Theme = "@style/AppTheme", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden, ScreenOrientation = ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.AdjustResize)]
    public class GetStartedActivity : AppCompatActivity, ITextWatcher, IOnKeyListener, IOnCountryPickerListener
    {
        //widgets
        private Button PrimaryButton;
        private EditText UserPhoneText;
        private CircleImageView countryFlagImg;
        private LinearLayout CCLayout;
        private TextView CCTV;
        private Android.Support.V7.Widget.Toolbar mToolbar;

        //dialogs
        private Android.Support.V7.App.AlertDialog alertDialog;
        private Android.Support.V7.App.AlertDialog.Builder alertBuilder;

        //country picker
        public static string phoneNumber;
        private CountryPicker.Builder builder;
        private CountryPicker picker;
        private Country country1;
        private string country_code;

        //shared preference
        ISharedPreferences preferences = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);
        ISharedPreferencesEditor editor;

        internal static GetStartedActivity Instance { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.getting_started_layout);
            Instance = this;
            
           
            InitControls();
        }
       
        protected override void OnResume()
        {
            base.OnResume();
            
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
                PrimaryButton.SetTextColor(Color.White);
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

            mToolbar = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.getting_started_toolbar);
            if (mToolbar != null)
                SetSupportActionBar(mToolbar);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.Title = "";   

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
            country1 = picker.CountryFromSIM;
            country_code = country1.Code;
            countryFlagImg.SetBackgroundResource(country1.Flag);
            CCTV.Text = country1.DialCode;
            
            UserPhoneText = (EditText)FindViewById(Resource.Id.user_phone_edittext);
            UserPhoneText.AddTextChangedListener(this);
            UserPhoneText.SetOnKeyListener(this);

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
                    ShowProgressDialog();
                    if (!ValidatePhoneNumberAndCode())
                    {
                        return;
                    }
                }
            };
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
        }

        private bool ValidatePhoneNumberAndCode()
        {
            
            if (string.IsNullOrEmpty(country_code))
            {
                return false;
            }

            PhoneNumberUtil phoneUtil = PhoneNumberUtil.Instance;
            Phonenumber.PhoneNumber phoneProto = null;
            try
            {
                phoneProto = phoneUtil.Parse(UserPhoneText.Text, country_code);
            }
            catch(NumberParseException npe)
            {
                Toast.MakeText(this, $"error: {npe.Message}", ToastLength.Short).Show();
            }

            bool isValid = phoneUtil.IsValidNumber(phoneProto);

            if (isValid)
            {
                CloseProgressDialog();
                //international format
                var int_format = phoneUtil.Format(phoneProto, PhoneNumberUtil.PhoneNumberFormat.International);

                //normal format
                string phone = CCTV.Text + UserPhoneText.Text;

                SaveToSharedPreference(int_format, phoneProto.ToString());

                Intent myintent = new Intent(this, typeof(PhoneValidationActivity));
                StartActivity(myintent);
                OverridePendingTransition(Resource.Animation.slide_up_anim, Resource.Animation.slide_up_out);
                return true;
            }
            else
            {
                CloseProgressDialog();
                Org.Aviran.CookieBar2.CookieBar.Build(this)
                    .SetTitle("Error")
                    .SetMessage("Invalid phone number")
                    .SetCookiePosition((int)GravityFlags.Bottom)
                    .Show();
                return false;
            }
        }

        private void SaveToSharedPreference(string int_format, string phoneProto)
        {
            editor = preferences.Edit();
            editor.PutString("int_format", int_format);
            editor.PutString("phoneProto", phoneProto);
            editor.Apply();
        }

        public void OnSelectCountry(Country country)
        {
            country_code = country.Code;
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

        public override bool OnSupportNavigateUp()
        {
            HideKeyboardHelper hideKeyboard = new HideKeyboardHelper(this);
            SupportFinishAfterTransition();
            return true;
        }

        public override void OnBackPressed()
        {
            HideKeyboardHelper hideKeyboard = new HideKeyboardHelper(this);
            SupportFinishAfterTransition();
            base.OnBackPressed();
            //Finish();
        }

    }
}