using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.JeevanDeshmukh.FancyBottomSheetDialogLib;
using Firebase.Auth;
using Firebase.Database;
using Java.Lang;
using Org.Json;
using Taxi__.Helpers;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Xamarin.Facebook.Login.Widget;
using static Xamarin.Facebook.GraphRequest;

namespace Taxi__.Fragments
{
    public class ProfileFragment : Android.Support.V4.App.DialogFragment, IFacebookCallback, IOnCompleteListener, IGraphJSONObjectCallback
    {
        private SessionManager sessionManager;
        private RelativeLayout profileRoot;
        private Android.Support.V7.Widget.Toolbar toolbar;
        private LoginButton fbLoginBtn;
        ICallbackManager callbackManager;
        FirebaseAuth mAuth;
        bool usingFirebase;
        MainActivity mainActivity;
        string email;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetStyle(StyleNormal, Resource.Style.FullScreenDG);
            sessionManager = SessionManager.GetInstance();
            mAuth = sessionManager.GetFirebaseAuth();
            mainActivity = MainActivity.Instance;
            callbackManager = CallbackManagerFactory.Create();
            // Create your fragment here
        }

        public static ProfileFragment Display(Android.Support.V4.App.FragmentManager fragmentManager, bool cancelable)
        {
            ProfileFragment profileFragment = new ProfileFragment();
            profileFragment.Show(fragmentManager, "tag");
            return profileFragment;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.profile_main, container, false);

            toolbar = (Android.Support.V7.Widget.Toolbar)view.FindViewById(Resource.Id.profile_main_toolbar);
            toolbar.Title = "Profile";

            fbLoginBtn = (LoginButton)view.FindViewById(Resource.Id.fb_btn);
            fbLoginBtn.SetReadPermissions(new List<string> { "public_profile", "email" });
            fbLoginBtn.Fragment = this;
            fbLoginBtn.RegisterCallback(callbackManager, this);

            sessionManager = SessionManager.GetInstance();

            profileRoot = (RelativeLayout)view.FindViewById(Resource.Id.profile_main_root);

            var phone = (EditText)view.FindViewById(Resource.Id.profile_phone);
            phone.Text = sessionManager.GetPhone();

            var email = (EditText)view.FindViewById(Resource.Id.profile_email);
            email.Text = sessionManager.GetEmail();

            var firstname = (EditText)view.FindViewById(Resource.Id.profile_firstname);
            firstname.Text = sessionManager.GetFirstname();

            var lastname = (EditText)view.FindViewById(Resource.Id.profile_lastname);
            lastname.Text = sessionManager.GetLastName();

            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            toolbar.NavigationClick += Toolbar_NavigationClick;
            base.OnViewCreated(view, savedInstanceState);   
        }

        private void Toolbar_NavigationClick(object sender, Android.Support.V7.Widget.Toolbar.NavigationClickEventArgs e)
        {
            toolbar.Title = "Profile";
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
                FirebaseUser user = task.Result as FirebaseUser;
                email = user.Email;
            }
            else
            {
                Toast.MakeText(Application.Context, task.Exception.Message, ToastLength.Short).Show();
            }
        }

        private void SetFacebookData(LoginResult loginResult)
        {
            GraphRequest graphRequest = NewMeRequest(loginResult.AccessToken, this);
            Bundle parameters = new Bundle();
            parameters.PutString("fields", "id,email,first_name,last_name");
            graphRequest.Parameters = parameters;
            graphRequest.ExecuteAsync();
        }

        public void OnCompleted(JSONObject @object, GraphResponse response)
        {
            try
            {
                string _email = response.JSONObject.GetString("email");
                string firstname = response.JSONObject.GetString("first_name");
                string lastname = response.JSONObject.GetString("last_name");
                new FancyBottomSheetDialog.Builder(mainActivity)
                    .SetTitle($"Welcome {firstname}")
                    .SetMessage("Your Facebook account is now linked to Cab360")
                    .IsCancellable(false)
                    .SetPositiveBtnText("OK")
                    .SetPositiveBtnBackground(Color.ParseColor("#FF5860F0"))
                    .SetIcon(Resource.Drawable.taxi, true)
                    .OnPositiveClicked(() =>
                    {

                    })
                    .Build();
            }
            catch (JSONException e)
            {
                e.PrintStackTrace();
            }
        }
    }
}