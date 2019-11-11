using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using Firebase.Auth;
using Firebase.Database;
using Java.Lang;
using Java.Util;
using System;
using Taxi__.Helpers;
using static Android.Views.View;

namespace Taxi__.Activities
{
    [Activity(Label = "Profile", Theme = "@style/AppTheme",ConfigurationChanges = Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.SmallestScreenSize, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.AdjustResize)]
    public class ProfileActivity : AppCompatActivity, ITextWatcher, IOnKeyListener
    {
        private Button ProfileNextBtn;
        private TextInputEditText EmailEditText;
        private TextInputEditText FirstNameEditText;
        private TextInputEditText LastNameEditText;

        private FirebaseAuth mAuth;
        private FirebaseDatabase database;
        private readonly SessionManager sessionManager = SessionManager.GetInstance();
        private string userPhone;

        //shared preference
        ISharedPreferences preferences = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);
        ISharedPreferencesEditor editor;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.profile_layout);

            mAuth = sessionManager.GetFirebaseAuth();
            database = sessionManager.GetDatabase();
            userPhone = sessionManager.GetFirebaseAuth().CurrentUser.PhoneNumber;

            InitControls();
            var profileToolbar = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.profile_toolbar);
            if (profileToolbar != null)
                SetSupportActionBar(profileToolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
        }

        public void AfterTextChanged(IEditable s)
        {
            CheckIfEmpty();
        }

        private void CheckIfEmpty()
        {
            var email = EmailEditText.Text;
            var fname = FirstNameEditText.Text;
            var lname = LastNameEditText.Text;

            ProfileNextBtn.Enabled = Android.Util.Patterns.EmailAddress.Matcher(email).Matches() && fname.Length >= 3 && lname.Length >= 3;
        }

        public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
        {
            
        }

        public bool OnKey(View v, [GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            var action = e.Action;
            if (action == KeyEventActions.Up)
            {
                CheckIfEmpty();
            }
            return false;
        }

        public void OnTextChanged(ICharSequence s, int start, int before, int count)
        {
            
        }

        private void InitControls()
        {
            //Terms and conditions
            var termsText = (TextView)FindViewById(Resource.Id.terms_tv);
            string first = "By signing up you agree to our ";
            string last = "Terms and Conditions";
            SpannableString str = new SpannableString(first + last);
            str.SetSpan(new StyleSpan(TypefaceStyle.Bold), first.Length, first.Length + last.Length, SpanTypes.ExclusiveExclusive);
            termsText.TextFormatted = str;

            //email
            EmailEditText = (TextInputEditText)FindViewById(Resource.Id.email_edittext);
            EmailEditText.SetOnKeyListener(this);
            EmailEditText.AddTextChangedListener(this);

            //firstname
            FirstNameEditText = (TextInputEditText)FindViewById(Resource.Id.fname_edittext);
            FirstNameEditText.SetOnKeyListener(this);
            FirstNameEditText.AddTextChangedListener(this);

            //lastname
            LastNameEditText = (TextInputEditText)FindViewById(Resource.Id.lname_edittext);
            LastNameEditText.SetOnKeyListener(this);
            LastNameEditText.AddTextChangedListener(this);
            ProfileNextBtn = (Button)FindViewById(Resource.Id.profile_prim_btn);
            ProfileNextBtn.Click += (s1, e1) =>
            {
                var email = EmailEditText.Text.Trim();
                var firstname = FirstNameEditText.Text.Trim();
                var lastname = LastNameEditText.Text.Trim();


                HashMap userMap = new HashMap();
                userMap.Put("email", email);
                userMap.Put("phone", userPhone);
                userMap.Put("firstname", firstname);
                userMap.Put("lastname", lastname);
                userMap.Put("timestamp", DateTime.UtcNow.ToString());

                DatabaseReference userReference = database.GetReference("Taxify_users/" + mAuth.CurrentUser.PhoneNumber);
                userReference.SetValue(userMap);
                userReference.KeepSynced(true);

                SaveToSharedPreference(email, userPhone, firstname, lastname);

                var intent = new Intent(this, typeof(MainActivity));
                intent.PutExtra("isPhoneAuthenticated", true);
                intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.ClearTask | ActivityFlags.NewTask);
                StartActivity(intent);
                OverridePendingTransition(Resource.Animation.slide_up_anim, Resource.Animation.slide_up_out);
                Finish();

            };
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

        public override bool OnSupportNavigateUp()
        {
            Finish();
            return true;
        }
    }
}