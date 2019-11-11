using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Taxi__.Helpers;

namespace Taxi__.Activities
{
    [Activity(Label = "Profile", Theme ="@style/AppTheme", ScreenOrientation =Android.Content.PM.ScreenOrientation.Portrait, ConfigurationChanges =Android.Content.PM.ConfigChanges.ScreenSize| Android.Content.PM.ConfigChanges.Orientation, NoHistory =false, MainLauncher =false)]
    public class MainProfileActivity : AppCompatActivity
    {
        private SessionManager sessionManager;
        private RelativeLayout profileRoot;
        ISharedPreferencesEditor editor;
        ISharedPreferences preferences = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.profile_main);
            InitWidgets();
        }

        private void InitWidgets()
        {
            //toolbar
            var toolbar = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.profile_main_toolbar);
            if (toolbar != null)
                SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            sessionManager = SessionManager.GetInstance();

            profileRoot = (RelativeLayout)FindViewById(Resource.Id.profile_main_root);

            var phone = (EditText)FindViewById(Resource.Id.profile_phone);
            phone.Text = sessionManager.GetPhone();

            var email = (EditText)FindViewById(Resource.Id.profile_email);
            email.Text = sessionManager.GetEmail();

            var firstname = (EditText)FindViewById(Resource.Id.profile_firstname);
            firstname.Text = sessionManager.GetFirstname();

            var lastname = (EditText)FindViewById(Resource.Id.profile_lastname);
            lastname.Text = sessionManager.GetLastName();

        }
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.sign_out_menu, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_logout:
                    //Snackbar.Make(profileRoot, "Helloworld", Snackbar.LengthShort).Show();
                    sessionManager.SignOut();
                    editor = preferences.Edit();
                    editor.Clear();
                    editor.Commit();
                    var intent = new Intent(this, typeof(GetStartedActivity));
                    intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.ClearTop | ActivityFlags.NewTask);
                    StartActivity(intent);
                    OverridePendingTransition(Resource.Animation.slide_up_anim, Resource.Animation.slide_up_out);
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override bool OnSupportNavigateUp()
        {
            //Finish();
            return true;
        }
    }
}