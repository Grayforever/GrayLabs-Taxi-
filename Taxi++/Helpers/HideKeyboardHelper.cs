using Android.App;
using Android.Content;
using Android.Views.InputMethods;
using Android.Widget;
using System;

namespace Taxi__.Helpers
{
    public class HideKeyboardHelper: Java.Lang.Object
    {
        public HideKeyboardHelper(Activity activity)
        {
            try
            {
                InputMethodManager inputMethodManager = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);
                inputMethodManager.HideSoftInputFromWindow(activity.CurrentFocus.ApplicationWindowToken, 0);
            }
            catch(Exception e)
            {
                Toast.MakeText(activity, e.Message, ToastLength.Short).Show();
            }
        }
    }
}