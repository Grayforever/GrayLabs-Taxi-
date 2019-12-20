using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Taxi__.Fragments
{
    public class NoNetworkFragment : Android.Support.V4.App.DialogFragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetStyle(StyleNormal, Resource.Style.FullScreenDG);
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            var view = inflater.Inflate(Resource.Layout.no_network_layout, container, false);
            var parentRoot = view.FindViewById<RelativeLayout>(Resource.Id.no_net_root);
            var tryBtn = view.FindViewById<Button>(Resource.Id.no_net_prim_btn);
            tryBtn.Click += (s1, e1) =>
            {
                Snackbar.Make(parentRoot, "lorem ipsum", Snackbar.LengthShort).Show();
            };
            return view;
        }

        public static NoNetworkFragment Display(Android.Support.V4.App.FragmentManager manager)
        {
            NoNetworkFragment noNetworkFragment = new NoNetworkFragment();

            noNetworkFragment.Cancelable = false;
            noNetworkFragment.Show(manager, "Hello");
            return noNetworkFragment;
        }
    }
}