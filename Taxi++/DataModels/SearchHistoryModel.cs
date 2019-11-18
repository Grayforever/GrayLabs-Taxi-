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
    public class SearchHistoryModel
    {
        public string PlaceName { get; set; }
        public int PlaceId { get; set; }
    }
}