using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Com.JeevanDeshmukh.FancyBottomSheetDialogLib;
using Com.Mukesh.CountryPickerLib;
using Firebase.Auth;
using Firebase.Database;
using Java.Lang;
using Refractored.Controls;
using System;
using System.Collections.Generic;
using Taxi__.EventListeners;
using Taxi__.Helpers;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;

namespace Taxi__.Activities
{
    [Activity(Label = "OnboardingActivity", Theme = "@style/AppTheme.MainScreen", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden, ScreenOrientation = ScreenOrientation.Portrait)]
    public class OnboardingActivity : AppCompatActivity, IFacebookCallback, IValueEventListener, IOnFailureListener, IOnSuccessListener
    {
        private RelativeLayout mRelativeLayout;
        private LinearLayout mLinearLayout;
        private LinearLayout ccLinear;
        private EditText mEditText;
        private CircleImageView countryFlagImg;
        FloatingActionButton mGoogleFab, mFacebookFab;
        private CookieBarHelper helper;
        SessionManager sessionManager;

        public const int RequestCode = 100;
        public const int RequestPermission = 200;
        private bool usingFirebase;
        private CountryPicker.Builder builder;
        private CountryPicker picker;
        private Country country;
        private FirebaseAuth auth;

        private ICallbackManager callbackManager;

        //shared preference
        ISharedPreferences preferences = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);
        ISharedPreferencesEditor editor;
        private string userID;

        private TaskCompletionStatusListener taskCompletionStatusListener = new TaskCompletionStatusListener();
        private AlertDialogHelper dialogHelper;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.onboarding_layout);
            GetWidgets();
            
            helper = new CookieBarHelper(this);
            dialogHelper = new AlertDialogHelper(this);
            sessionManager = SessionManager.GetInstance();
            auth = sessionManager.GetFirebaseAuth();
            callbackManager = CallbackManagerFactory.Create();
            LoginManager.Instance.RegisterCallback(callbackManager, this);
        }

        private bool CheckLocationPermission()
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
            mGoogleFab = (FloatingActionButton)FindViewById(Resource.Id.fab_google);
            mGoogleFab.Click += MGoogleFab_Click;
            mFacebookFab = (FloatingActionButton)FindViewById(Resource.Id.fab_fb);
            mFacebookFab.Click += MFacebookFab_Click;

            mRelativeLayout = (RelativeLayout)FindViewById(Resource.Id.onboard_root);
            mRelativeLayout.RequestFocus();
            mRelativeLayout.Click += MRelativeLayout_Click;

            mLinearLayout = (LinearLayout)FindViewById(Resource.Id.mLinear_view_2);
            ccLinear = (LinearLayout)FindViewById(Resource.Id.cc_layout_2);

            mEditText = (EditText)FindViewById(Resource.Id.user_phone_edittext2);
            mEditText.SetCursorVisible(false);
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

        private void MFacebookFab_Click(object sender, EventArgs e)
        {
            LoginManager.Instance.LogInWithReadPermissions(this, new List<string> { "public_profile", "email" });
        }

        private void MRelativeLayout_Click(object sender, EventArgs e)
        {
            GetSharedIntent();
        }

        private void MGoogleFab_Click(object sender, EventArgs e)
        {
            helper.ShowCookieBar("Info", "google login coming soon");
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

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            callbackManager.OnActivityResult(requestCode, (int)resultCode, data);
        }

        private void GetSharedIntent()
        {
            var sharedIntent = new Intent(this, typeof(GetStartedActivity));
            Android.Support.V4.Util.Pair p1 = Android.Support.V4.Util.Pair.Create(countryFlagImg, "cc_trans");
            Android.Support.V4.Util.Pair p2 = Android.Support.V4.Util.Pair.Create(mEditText, "edittext_trans");

            ActivityOptionsCompat activityOptions = ActivityOptionsCompat.MakeSceneTransitionAnimation(this, p1, p2);
            StartActivity(sharedIntent, activityOptions.ToBundle());
        }

        protected override void OnResume()
        {
            base.OnResume();
            CheckLocationPermission();
        }

        public void OnCancel()
        {
            helper.ShowCookieBar("Info", "facebook login canceled");
        }

        public void OnError(FacebookException error)
        {
            helper.ShowCookieBar("Facebook Error", error.Message);
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            
            if (!usingFirebase)
            {
                dialogHelper.ShowDialog();
                usingFirebase = true;
                LoginResult loginResult = result as LoginResult;

                var credentials = FacebookAuthProvider.GetCredential(loginResult.AccessToken.Token);
                auth.SignInWithCredential(credentials)
                    .AddOnSuccessListener(this, this)
                    .AddOnFailureListener(this, this);
            }
            else
            {
                usingFirebase = false;
                userID = auth.CurrentUser.Uid;
                CheckIfUserExists();
            }
        }

        public void OnFailure(Java.Lang.Exception e)
        {
            dialogHelper.CloseDialog();
            helper.ShowCookieBar("Error", e.Message);
        }

        private void CheckIfUserExists()
        {
            DatabaseReference userRef = sessionManager.GetDatabase().GetReference("Taxify_users");
            userRef.OrderByKey().EqualTo(userID).AddListenerForSingleValueEvent(this);
        }

        private void SaveToSharedPreference(string email, string phone, string firstname, string lastname)
        {
            editor = preferences.Edit();
            editor.PutString("email", email);
            editor.PutString("firstname", firstname);
            editor.PutString("lastname", lastname);
            editor.PutString("phone", phone);

            editor.Apply();
        }

        public void OnCancelled(DatabaseError error)
        {
            dialogHelper.CloseDialog();
            helper.ShowCookieBar("Error", error.Message);
        }

        public void OnDataChange(DataSnapshot snapshot)
        {
            dialogHelper.CloseDialog();
            if (snapshot.Value != null)
            {
                try
                {
                    var child = snapshot.Child(userID);
                    var email = child?.Child("email").Value.ToString();
                    var phone = child?.Child("phone").Value.ToString();
                    var firstname = child?.Child("firstname").Value.ToString();
                    var lastname = child?.Child("lastname").Value.ToString();

                    SaveToSharedPreference(email, phone, firstname, lastname);

                    var intent = new Intent(this, typeof(MainActivity));
                    intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.ClearTop | ActivityFlags.NewTask);
                    StartActivity(intent);
                    dialogHelper.CloseDialog();
                    Finish();
                }
                catch
                {

                }
            }
            else
            {
                
                new FancyBottomSheetDialog.Builder(this)
                       .SetTitle("Info")
                       .SetMessage("Your Facebook is not linked to any Cab360 account, please sign up first.")
                       .IsCancellable(false)
                       .SetPositiveBtnText("OK")
                       .SetPositiveBtnBackground(Color.ParseColor("#FF5860F0"))
                       .SetIcon(Resource.Drawable.taxi, true)
                       .OnPositiveClicked(() =>
                       {

                           auth.SignOut();
                           LoginManager.Instance.LogOut();
                       })
                       .Build();
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            CheckLocationPermission();
            
        }

        
    }
}