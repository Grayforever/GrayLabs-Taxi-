using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using System;

namespace Taxi__.Fragments
{
    public class FindingDriverDialog : Android.Support.V4.App.DialogFragment
    {
        private BottomSheetBehavior behavior_finder;

        private RelativeLayout rl_bottomSheet;

        private TextView from_tv;
        private TextView to_tv;
        private TextView eta_tv;
        private TextView price_tv;

        private static string _from, _to, _eta;
        private static double _fares;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetStyle(StyleNormal, Resource.Style.FullScreenDialogTheme);
            // Create your fragment here
        }

        public static FindingDriverDialog Display(Android.Support.V4.App.FragmentManager fragmentManager, bool cancelable, string from, string to, string eta, double fares)
        {
            
            FindingDriverDialog findingDriver = new FindingDriverDialog
            {
                Cancelable = cancelable
            };

            _from = from;
            _to = to;
            _eta = eta;
            _fares = fares;

            findingDriver.Show(fragmentManager, "TAG");
            return findingDriver;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.finding_driver_nearby, container, false);

            var mlinear_ = (LinearLayout)view.FindViewById(Resource.Id.mLinear_finder);
            mlinear_.Click += Mlinear__Click;

            from_tv = (TextView)view.FindViewById(Resource.Id.mypos_tv);
            to_tv = (TextView)view.FindViewById(Resource.Id.dest_tv);
            eta_tv = (TextView)view.FindViewById(Resource.Id.eta_tv);
            price_tv = (TextView)view.FindViewById(Resource.Id.price_tv);

            from_tv.Text = _from;
            to_tv.Text = _to;
            eta_tv.Text = _eta;
            price_tv.Text = $"Gh¢ {_fares.ToString()}";

            var btnCancel = (Button)view.FindViewById(Resource.Id.canc_btn);
            btnCancel.Click += (s1, e1) =>
            {
                MainActivity.Instance.ReverseTrip();
                MainActivity.Instance.RideRequestEvent.CancelRequestAsync();
                
                Dismiss();
            };

            rl_bottomSheet = (RelativeLayout)view.FindViewById(Resource.Id.finder_rl);
            behavior_finder = BottomSheetBehavior.From(rl_bottomSheet);

            return view;
        }

        private void Mlinear__Click(object sender, EventArgs e)
        {
            switch (behavior_finder.State)
            {
                case BottomSheetBehavior.StateExpanded:
                    behavior_finder.State = BottomSheetBehavior.StateCollapsed;
                    break;

                case BottomSheetBehavior.StateCollapsed:
                    behavior_finder.State = BottomSheetBehavior.StateExpanded;
                    break;

                default:
                    behavior_finder.State = BottomSheetBehavior.StateCollapsed;
                    break;
            }
        }
    }
}