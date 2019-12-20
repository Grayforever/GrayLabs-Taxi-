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

namespace Taxi__.Helpers
{
    public class AlertDialogHelper : Java.Lang.Object
    {
        Activity _activity;
        //dialogs
        Android.Support.V7.App.AlertDialog alertDialog;
        Android.Support.V7.App.AlertDialog.Builder alertBuilder;

        public AlertDialogHelper(Activity activity)
        {
            _activity = activity;
        }

        public void ShowDialog()
        {
            alertBuilder = new Android.Support.V7.App.AlertDialog.Builder(_activity);
            alertBuilder.SetView(Resource.Layout.progress_dialog_layout);
            alertBuilder.SetCancelable(false);
            alertDialog = alertBuilder.Show();
        }

        public void CloseDialog()
        {
            if (alertDialog != null)
            {
                alertDialog.Dismiss();
                alertDialog = null;
                alertBuilder = null;
            }
        }
    }
}