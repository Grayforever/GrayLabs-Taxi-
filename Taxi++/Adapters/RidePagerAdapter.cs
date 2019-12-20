using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using Taxi__.DataModels;

namespace Taxi__.Adapters
{
    public class RidePagerAdapter : PagerAdapter
    {
        Activity activity;
        List<RideTypeDataModel> ride_type_list;
        LayoutInflater inflater;

        public RidePagerAdapter(Activity _activity, List<RideTypeDataModel> _ride_type_list)
        {
            activity = _activity;
            ride_type_list = _ride_type_list;
        }
        public override int Count
        {
            get { return ride_type_list.Count(); }
        }

        public override bool IsViewFromObject(View view, Java.Lang.Object @object)
        {
            return view.Equals(@object);
        }

        public override Java.Lang.Object InstantiateItem(ViewGroup container, int position)
        {
            inflater = LayoutInflater.From(activity);
            View view = inflater.Inflate(Resource.Layout.ride_type_layout, container, false);
            ImageView imageView;
            TextView titleText, descText;

            imageView = (ImageView)view.FindViewById(Resource.Id.ride_img);
            titleText = (TextView)view.FindViewById(Resource.Id.txt_ride_type);
            descText = (TextView)view.FindViewById(Resource.Id.txt_ride_price);

            imageView.SetImageResource(ride_type_list[position].Image);
            titleText.Text = ride_type_list[position].RideType;
            descText.Text = ride_type_list[position].RidePrice;
            container.AddView(view, 0);
            return view;
        }

        public override void DestroyItem(ViewGroup container, int position, Java.Lang.Object @object)
        {
            container.RemoveView((View)@object);
        }
    }
}