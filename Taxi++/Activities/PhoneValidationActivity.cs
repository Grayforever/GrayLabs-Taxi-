using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Text;
using Android.Text.Style;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using Com.Goodiebag.Pinview;
using Firebase.Auth;
using Firebase.Database;
using Java.Lang;
using Plugin.Connectivity;
using System;
using Taxi__.Fragments;
using Taxi__.Helpers;
using static Com.Goodiebag.Pinview.Pinview;
using Java.Util;

namespace Taxi__.Activities
{
    [Activity(Label ="Enter code", Theme = "@style/AppTheme",ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.SmallestScreenSize, ScreenOrientation = ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.AdjustResize)]
    public class PhoneValidationActivity : AppCompatActivity, IPinViewEventListener, IOnCompleteListener, IValueEventListener
    {

        //Views
        private Android.Support.V7.Widget.Toolbar VeriToolbar;
        public Pinview CodePinView;
        private TextView EnterCodeTV;
        private TextView TimerTV;
        private Button NextButton;
        private FirebaseAuth mAuth;

        //dialogs
        private Android.Support.V7.App.AlertDialog alertDialog;
        private Android.Support.V7.App.AlertDialog.Builder builder;

        internal static PhoneValidationActivity Instance { get; set; }

        CookieBarHelper barHelper;

        private string VerificationID, userID, int_format, phoneProto;
        FirebaseDatabase database;
        private SessionManager sessionManager;

        //shared preference
        ISharedPreferences preferences = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);
        ISharedPreferencesEditor editor;
        private string email;
        private string userPhone;
        private string firstname;
        private string lastname;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.phone_validation_layout);

            Instance = this;
            sessionManager = SessionManager.Instance;
            database = sessionManager.GetDatabase();
            int_format = sessionManager.GetIntFormat();
            phoneProto = sessionManager.GetPhoneProto();
            InitControls();
            barHelper = new CookieBarHelper(this);
        }

        private void InitControls()
        {
            //Toolbar props
            VeriToolbar = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.prim_toolbar1);
            if (VeriToolbar != null)
                SetSupportActionBar(VeriToolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);

            //Views
            CodePinView = (Pinview)FindViewById(Resource.Id.phone_pinView);
            CodePinView.SetPinViewEventListener(this);
            EnterCodeTV = (TextView)FindViewById(Resource.Id.enter_code_tv);

            TimerTV = (TextView)FindViewById(Resource.Id.timer_tv);
            TimerTV.Click += (s2, e2) =>
            {
                //
            };

            NextButton = (Button)FindViewById(Resource.Id.prim_btn1);
            NextButton.Click += NextButton_Click;

            var first = "Text message sent to ";

            SpannableString str = new SpannableString(first + int_format);
            str.SetSpan(new StyleSpan(TypefaceStyle.Bold), first.Length, first.Length + int_format.Length, SpanTypes.ExclusiveExclusive);
            //str.SetSpan(new ForegroundColorSpan(Color.Rgb(88, 96, 240)), first.Length, first.Length + int_format.Length, SpanTypes.ExclusiveExclusive);

            EnterCodeTV.TextFormatted = str;

            SessionManager sessionManager = SessionManager.GetInstance();
            sessionManager.SendVerificationCode(int_format, Instance);
        }

        private void NextButton_Click(object sender, EventArgs e)
        {

            var otpCode = CodePinView.Value;
            if (!CrossConnectivity.Current.IsConnected)
            {
                Android.Support.V4.App.DialogFragment dialogFragment = new NoNetworkFragment();
                dialogFragment.Show(SupportFragmentManager, "no network");
            }
            else
            {
                VerificationCode(VerificationID, otpCode);
            }
        }

        public override bool OnSupportNavigateUp()
        {
            Finish();
            return true;
        }

        public void OnDataEntered(Pinview otpView, bool textFromUSer)
        {
            NextButton.Enabled = CodePinView.Value.Length == 6;
        }

        public void VerificationCode(string strVerificationId, string otpCode)
        {
            try
            {
                VerificationID = strVerificationId;
                PhoneAuthCredential credential = PhoneAuthProvider.GetCredential(strVerificationId, otpCode);
                InitializeCredentials(credential);
            }
            catch (IllegalArgumentException iae)
            {
                Log.Debug("illegalArgument on verificationID", iae.Message);
            }
        }

        private void InitializeCredentials(PhoneAuthCredential credential)
        {

            SessionManager sessionManager = SessionManager.GetInstance();
            mAuth = sessionManager.GetFirebaseAuth();
            mAuth.SignInWithCredential(credential)
                .AddOnCompleteListener(this, this);
        }

        public void OnComplete(Task task)
        {
            userID = sessionManager.GetCurrentUser().Uid;
            CheckIfUserExists(); 
        }

        private void CheckIfUserExists()
        {
            SessionManager sessionManager = new SessionManager();
            DatabaseReference userRef = sessionManager.GetDatabase().GetReference("Taxify_users");
            userRef.OrderByKey().EqualTo(userID).AddListenerForSingleValueEvent(this);
        }

        public void ShowProgressDialog()
        {
            builder = new Android.Support.V7.App.AlertDialog.Builder(this);
            builder.SetView(Resource.Layout.progress_dialog_layout);
            builder.SetCancelable(false);
            alertDialog = builder.Show();
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

        public void OnCancelled(DatabaseError error)
        {
            
        }

        public void OnDataChange(DataSnapshot snapshot)
        {

            if (snapshot.Value != null)
            {
                var child = snapshot.Child(userID);
                try
                {
                    email = child?.Child("email").Value.ToString();
                    userPhone = child?.Child("phone").Value.ToString();
                    firstname = child?.Child("firstname").Value.ToString();
                    lastname = child?.Child("lastname").Value.ToString();

                    SaveToSharedPreference(email,
                                           userPhone,
                                           firstname,
                                           lastname);

                    CloseProgressDialog();
                    var intent = new Intent(this, typeof(MainActivity));
                    intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.ClearTop | ActivityFlags.NewTask);
                    StartActivity(intent);
                    Finish();
                }
                catch
                {

                }

            }
            else
            {
                StartActivity(new Intent(this, typeof(ProfileActivity)));
                OverridePendingTransition(Resource.Animation.slide_up_anim, Resource.Animation.slide_up_out);
            }

        }

        void SaveToSharedPreference(string email, string phone, string firstname, string lastname)
        {

            editor = preferences.Edit();
            editor.PutString("email", email);
            editor.PutString("firstname", firstname);
            editor.PutString("lastname", lastname);
            editor.PutString("phone", phone);

            editor.Apply();
        }  
    }

}