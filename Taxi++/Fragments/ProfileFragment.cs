using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using FFImageLoading;
using Firebase.Auth;
using Org.Json;
using Refractored.Controls;
using System;
using System.Collections.Generic;
using Taxi__.Activities;
using Taxi__.Constants;
using Taxi__.Helpers;
using Taxi__.Utils;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Xamarin.Facebook.Login.Widget;
using static Android.Views.View;
using static Xamarin.Facebook.GraphRequest;

namespace Taxi__.Fragments
{
    public class ProfileFragment : Android.Support.V4.App.Fragment, IFacebookCallback, IOnCompleteListener, IGraphJSONObjectCallback
    {
        private SessionManager sessionManager;
        private RelativeLayout profileRoot;
        private Android.Support.V7.Widget.Toolbar toolbar;
        private LoginButton fbLoginBtn;
        private ICallbackManager callbackManager;
        private FirebaseAuth mAuth;
        private bool usingFirebase;
        MainActivity mainActivity;
        private string email;
        private CircleImageView Profile_img;
        private RelativeLayout homeRelative, workRelative;
        //shared preference
        ISharedPreferences preferences = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);
        ISharedPreferencesEditor editor;

        private Button LogOutBtn;

        //dialogs
        private Android.App.AlertDialog alertDialog;
        private Android.App.AlertDialog.Builder builder;

        private TextView menuBtn;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            sessionManager = SessionManager.GetInstance();
            mAuth = sessionManager.GetFirebaseAuth();
            mainActivity = MainActivity.Instance;
            callbackManager = CallbackManagerFactory.Create();
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.profile_main, container, false);

            //mainActivity.LockDrawer();
            sessionManager = SessionManager.GetInstance();
            toolbar = (Android.Support.V7.Widget.Toolbar)view.FindViewById(Resource.Id.custom_toolbar);

            fbLoginBtn = (LoginButton)view.FindViewById(Resource.Id.fb_btn);
            fbLoginBtn.SetPermissions(new List<string> { "public_profile", "email" });
            fbLoginBtn.Fragment = this;
            fbLoginBtn.RegisterCallback(callbackManager, this);

            homeRelative = (RelativeLayout)view.FindViewById(Resource.Id.add_home_rl);
            workRelative = (RelativeLayout)view.FindViewById(Resource.Id.add_work_rl);
            homeRelative.Click += HomeRelative_Click; workRelative.Click += WorkRelative_Click;

            Profile_img = (CircleImageView)view.FindViewById(Resource.Id.profile_ivew);

            var fbID = sessionManager.GetFbProfilePic();

            LogOutBtn = (Button)view.FindViewById(Resource.Id.log_out_btn);
            LogOutBtn.Click += LogOutBtn_Click;

            profileRoot = (RelativeLayout)view.FindViewById(Resource.Id.profile_main_root);

            var firstname = sessionManager.GetFirstname();
            var lastname = sessionManager.GetLastName();
            var email = sessionManager.GetEmail();
            var isLinked = sessionManager.IsProviderLinked();

            var phone = (TextView)view.FindViewById(Resource.Id.profile_txt2);
            phone.Text = sessionManager.GetPhone();

            var fullname = (TextView)view.FindViewById(Resource.Id.profile_txt1);
            fullname.Text = $"{firstname} {lastname}";

            var rideReceiptTxt = (TextView)view.FindViewById(Resource.Id.textView_ride);
            var first = "Ride receipt will be sent to ";
            SpannableString str = new SpannableString(first + email);
            str.SetSpan(new StyleSpan(TypefaceStyle.Bold), first.Length, first.Length + email.Length, SpanTypes.ExclusiveExclusive);
            rideReceiptTxt.TextFormatted = str;

            menuBtn = (TextView)view.FindViewById(Resource.Id.edit_menu);
            menuBtn.Click += MenuBtn_Click;
            int logintype = sessionManager.GetLogintype();

            switch (logintype)
            {
                case 0:
                    Toast.MakeText(Application.Context, "Phone auth", ToastLength.Short).Show();
                    if(isLinked == true)
                    {
                        fbLoginBtn.Visibility = ViewStates.Invisible;
                    }
                    break;

                case 1:
                    Toast.MakeText(Application.Context, "facebook auth", ToastLength.Short).Show();
                    mainActivity.RunOnUiThread(() => 
                    {
                        fbLoginBtn.Visibility = ViewStates.Invisible;
                        SetProfilePic(fbID, Profile_img);
                    });
                    break;

                case 2:
                    Toast.MakeText(Application.Context, "Google auth", ToastLength.Short).Show();
                    fbLoginBtn.Visibility = ViewStates.Invisible;
                    break;

                default:
                    Toast.MakeText(Application.Context, "No such data", ToastLength.Short).Show();
                    break;
            }
            return view;
        }

        private void WorkRelative_Click(object sender, EventArgs e)
        {
            PlaceTypeRequest(1);
        }

        private void HomeRelative_Click(object sender, EventArgs e)
        {
            PlaceTypeRequest(2);
        }

        private void PlaceTypeRequest(int index)
        {
            
        }

        private void MenuBtn_Click(object sender, EventArgs e)
        {
            PopupMenu popupMenu = new PopupMenu(mainActivity, menuBtn, GravityFlags.ClipVertical);
            popupMenu.MenuInflater.Inflate(Resource.Menu.profile_edit_menu, popupMenu.Menu);
            //popupMenu.Inflate(Resource.Menu.package_menu);
            popupMenu.MenuItemClick += (se, ev) =>
            {
                switch (ev.Item.ItemId)
                {
                    case Resource.Id.action_edit_firstname:

                        break;

                    case Resource.Id.action_edit_lastname:

                        break;

                    case Resource.Id.action_edit_email:

                        break;
                }
            };
            popupMenu.Show();
        }

        private async void SetProfilePic(string providerID, CircleImageView imageView)
        {
            try
            {
                await ImageService.Instance
                   .LoadUrl($"https://graph.facebook.com/{providerID}/picture?type=normal")
                   .LoadingPlaceholder("boy_new.png", FFImageLoading.Work.ImageSource.CompiledResource)
                   .Retry(3, 200)
                   .IntoAsync(imageView);
            }
            catch(Exception ex)
            {
                Toast.MakeText(Application.Context, $"Profile Error: {ex.Message}", ToastLength.Short).Show();
            }

        }

        private void LogOutBtn_Click(object sender, System.EventArgs e)
        {
            ShowLogoutDialog();
            
        }

        private void ShowLogoutDialog()
        {
            builder = new Android.App.AlertDialog.Builder(mainActivity);
            alertDialog = builder.Create();
            alertDialog.SetMessage("Do you want to log out?");
            alertDialog.SetButton("Yes", (s1, e1) =>
            {
                var auth = sessionManager.GetFirebaseAuth();
                editor = preferences.Edit();
                LoginManager.Instance.LogOut();
                auth.SignOut();
                editor.Clear();
                editor.Commit();

                var intent = new Intent(Application.Context, typeof(OnboardingActivity));
                intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.ClearTop | ActivityFlags.NewTask);
                StartActivity(intent);
                mainActivity.Finish();

            });

            alertDialog.SetButton2("No", (s2, e2) =>
            {
                alertDialog.Dismiss();
            });
            alertDialog.Show();
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            callbackManager.OnActivityResult(requestCode, (int)resultCode, data);
        }

        public void OnCancel()
        {
            
        }

        public void OnError(FacebookException error)
        {
            
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            LoginResult loginResult = result as LoginResult;
            if (!usingFirebase)
            {
                usingFirebase = true;
                var authCredential = FacebookAuthProvider.GetCredential(loginResult.AccessToken.Token);
                mAuth.CurrentUser.LinkWithCredential(authCredential)
                    .AddOnCompleteListener(this);
            }
            else
            {
                usingFirebase = false;
                SetFacebookData(loginResult);
            }
            
        }

        public void OnComplete(Task task)
        {
            if (task.IsSuccessful)
            {
                email = mAuth.CurrentUser.Email;

            }
            else
            {
                LoginManager.Instance.LogOut();
                Toast.MakeText(Application.Context, task.Exception.Message, ToastLength.Short).Show();
            }
        }

        private void SetFacebookData(LoginResult loginResult)
        {
            GraphRequest graphRequest = NewMeRequest(loginResult.AccessToken, this);
            Bundle parameters = new Bundle();
            parameters.PutString("fields", "id,email,first_name,last_name,picture");
            graphRequest.Parameters = parameters;
            graphRequest.ExecuteAsync();
        }

        public void OnCompleted(JSONObject @object, GraphResponse response)
        {
            try
            {
                string fbid = response.JSONObject.GetString("id");
                string _email = response.JSONObject.GetString("email");
                string firstname = response.JSONObject.GetString("first_name");
                string lastname = response.JSONObject.GetString("last_name");
                
                SetProfilePic(fbid, Profile_img);

            }
            catch (JSONException e)
            {
                e.PrintStackTrace();
            }
        }

        private async void UpdateIsLinkedAsync(bool isLinked)
        {
            var dbref = sessionManager.GetDatabase().GetReference("Taxify_users");
            await dbref.Child(mAuth.CurrentUser.Uid).Child("User_profile").Child("isLinkedWithAuth")
                .SetValueAsync(isLinked);

            editor = preferences.Edit();
            editor.PutBoolean("isLinked", isLinked);
            editor.Apply();

            fbLoginBtn.Visibility = ViewStates.Gone;
        }
    }
}