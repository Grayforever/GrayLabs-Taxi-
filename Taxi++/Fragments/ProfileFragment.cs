using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Taxi__.Helpers;

namespace Taxi__.Fragments
{
    public class ProfileFragment : Android.Support.V4.App.DialogFragment
    {
        private SessionManager sessionManager;
        private RelativeLayout profileRoot;
        private Android.Support.V7.Widget.Toolbar toolbar;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetStyle(StyleNormal, Resource.Style.FullScreenDG);
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
    }
}