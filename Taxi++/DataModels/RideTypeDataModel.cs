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

namespace Taxi__.DataModels
{
    public class RideTypeDataModel
    {
        public int Image { get; set; }
        public string RideType { get; set; }
        public string RidePrice { get; set; }
    }
}