using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Stripe.Android;
using Com.Stripe.Android.Model;
using Java.Lang;
using Taxi__.Activities;

namespace Taxi__.Helpers
{
    public class StripeTokenCallback : Java.Lang.Object, ITokenCallback
    {
        //public IntPtr Handle => throw new NotImplementedException();
        public Context _context = null;

        public StripeTokenCallback(Context context)
        {
            _context = context;
            return;
        }
        public new void Dispose()
        {
            return;
        }

        public void OnError(Java.Lang.Exception p0)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"Error: {p0.LocalizedMessage}");

            Toast.MakeText(_context, sb.ToString(), ToastLength.Short).Show();
            return;
        }

        public void OnSuccess(Token p0)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"Success: {p0.ToString()}");

            Toast.MakeText(_context, sb.ToString(), ToastLength.Long).Show();
            

            return;
        }
    }
}