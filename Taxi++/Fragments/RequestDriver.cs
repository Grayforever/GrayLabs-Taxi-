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
    public class RequestDriver : Android.Support.V4.App.DialogFragment
    {
        public event EventHandler CancelRequest;

        double mfares;
        Button cancelRequestButton;
        TextView faresText;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            var view = inflater.Inflate(Resource.Layout.request_driver, container, false);
            cancelRequestButton = (Button)view.FindViewById(Resource.Id.cancelrequestButton);
            cancelRequestButton.Click += CancelRequestButton_Click;
            faresText = (TextView)view.FindViewById(Resource.Id.faresText);
            faresText.Text = "$" + mfares.ToString();
            return view;
        }

        public RequestDriver(double fares)
        {
            mfares = fares;
        }

        void CancelRequestButton_Click(object sender, EventArgs e)
        {
            CancelRequest?.Invoke(this, new EventArgs());
        }

    }
}