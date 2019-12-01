using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;

namespace Taxi__.Fragments
{
    public class FindingDriverDialog : Android.Support.V4.App.DialogFragment
    {
        private BottomSheetBehavior behavior_finder;
        private RelativeLayout rl_bottomSheet;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetStyle(StyleNormal, Resource.Style.FullScreenDialogTheme);
            // Create your fragment here
        }

        public static FindingDriverDialog Display(Android.Support.V4.App.FragmentManager fragmentManager, bool cancelable)
        {
            FindingDriverDialog findingDriver = new FindingDriverDialog();
            findingDriver.Cancelable = cancelable;
            findingDriver.Show(fragmentManager, "TAG");
            return findingDriver;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.finding_driver_nearby, container, false);
            var btnCancel = (Button)view.FindViewById(Resource.Id.cancel_finding_btn);

            btnCancel.Click += (s1, e1) =>
            {
                Dismiss();
                MainActivity.Instance.ReverseTrip();
            };
            rl_bottomSheet = (RelativeLayout)view.FindViewById(Resource.Id.finder_rl);
            behavior_finder = BottomSheetBehavior.From(rl_bottomSheet);
            return view;
        }
    }
}