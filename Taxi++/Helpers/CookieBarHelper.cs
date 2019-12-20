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
    public class CookieBarHelper : Java.Lang.Object
    {
        Activity _activity;
        public CookieBarHelper(Activity activity)
        {
            _activity = activity;
        }

        public void ShowCookieBar(string title, string message)
        {
            Org.Aviran.CookieBar2.CookieBar.Build(_activity)
               .SetTitle(title)
               .SetMessage(message)
               .SetCookiePosition((int)GravityFlags.Bottom)
               .Show();
        }
    }
}