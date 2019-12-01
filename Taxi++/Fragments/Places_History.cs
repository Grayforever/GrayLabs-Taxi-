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

namespace Taxi__.Fragments
{
    public class Places_History : Android.Support.V4.App.DialogFragment
    {
        
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetStyle(StyleNormal, Resource.Style.FullScreenDG);
        }

        public static Places_History Display(Android.Support.V4.App.FragmentManager fragmentManager, bool cancelable)
        {
            Places_History places_History = new Places_History();
            places_History.Show(fragmentManager, "History");

            return places_History;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.places_layout, container, false);
            return view;
        }
    }
}