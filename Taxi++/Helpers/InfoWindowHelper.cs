using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Taxi__.Helpers
{
    public class InfoWindowHelper : Java.Lang.Object, GoogleMap.IInfoWindowAdapter
    {
        private Activity mContext;
        private View mView;
        private Marker mMarker;

        public InfoWindowHelper(Activity context)
        {
            mContext = context;
            mView = mContext.LayoutInflater.Inflate(Resource.Layout.info_window_layout, null);
        }

        public View GetInfoContents(Marker marker)
        {
            if (marker == null)
                return null;

            mMarker = marker;
            
            var text_time = (TextView)mView.FindViewById(Resource.Id.m_tv_1);
            var text_location = (TextView)mView.FindViewById(Resource.Id.info_txt);
            var linear = (LinearLayout)mView.FindViewById(Resource.Id.linearLayout1);

            text_time.Text = marker.Snippet;
            text_location.Text = marker.Title;

            return mView;
        }

        public View GetInfoWindow(Marker marker)
        {
            return null;
        }
    }
}