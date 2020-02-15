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
    public class PaymentsFragment : Android.Support.V4.App.Fragment
    {
        private BottomSheetBehavior mBehavior;
        private RelativeLayout mBottomsheetRoot;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.payments_layout, container, false);

            //add payment bottomsheet
            mBottomsheetRoot = (RelativeLayout)view.FindViewById(Resource.Id.payment_sheet_root);
            mBehavior = BottomSheetBehavior.From(mBottomsheetRoot);
            mBehavior.State = BottomSheetBehavior.StateHidden;

            var cancelTxt = (TextView)view.FindViewById(Resource.Id.cancel_txt);
            cancelTxt.Click += CancelTxt_Click;
            var addPaymentBtn = (Button)view.FindViewById(Resource.Id.add_new_payment_btn);
            addPaymentBtn.Click += AddPaymentBtn_Click;

            var momoBtn = (RelativeLayout)view.FindViewById(Resource.Id.momo_relative);
            momoBtn.Click += MomoBtn_Click;

            var ccBtn = (RelativeLayout)view.FindViewById(Resource.Id.cc_relative);
            ccBtn.Click += CcBtn_Click;

            return view;
        }

        private void CcBtn_Click(object sender, EventArgs e)
        {
            Toast.MakeText(this.Context, "Credit card added successfully", ToastLength.Short).Show();

        }

        private void MomoBtn_Click(object sender, EventArgs e)
        {
            Toast.MakeText(this.Context, "Momo added successfully", ToastLength.Short).Show();
        }

        private void CancelTxt_Click(object sender, EventArgs e)
        {
            mBehavior.Hideable = true;
            mBehavior.State = BottomSheetBehavior.StateHidden;
            
        }

        private void AddPaymentBtn_Click(object sender, EventArgs e)
        {
            mBehavior.State = BottomSheetBehavior.StateExpanded;
            mBehavior.Hideable = false;
        }
    }
}