using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Stripe.Android;
using Com.Stripe.Android.View;
using System.Collections.Generic;
using Taxi__.Adapters;
using Taxi__.DataModels;
using Taxi__.Fragments;
using Taxi__.Helpers;

namespace Taxi__.Activities
{
    [Activity(Label = "Add your card", Theme ="@style/AppTheme", MainLauncher =false, WindowSoftInputMode = SoftInput.AdjustResize, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class PaymentsActiviity : AppCompatActivity
    {
        //dialogs
        private Android.Support.V7.App.AlertDialog alertDialog;
        private Android.Support.V7.App.AlertDialog.Builder builder;
        private RecyclerView mRecyclerView;
        private PackageAdapter mAdapter;
        private RecyclerView.LayoutManager mLayoutManager;

        List<PaymentDataModels> mRecyclerList;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.payment_activity);
            InitWidgets();
            CreateData();
            GetData();
        }

        private void InitWidgets()
        {
            var payments_main_toolbar = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.payments_main_toolbar);
            if (payments_main_toolbar != null)
                SetSupportActionBar(payments_main_toolbar);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            mRecyclerView = (RecyclerView)FindViewById(Resource.Id.payments_recycler);

            var mAddPaymentBtn = (Button)FindViewById(Resource.Id.material_btn_add_card);
            mAddPaymentBtn.Click += MAddPaymentBtn_Click;
        }

        private void MAddPaymentBtn_Click(object sender, System.EventArgs e)
        {
            AddPaymentMethod.display(SupportFragmentManager);
            
        }

        void GetData()
        {
            mLayoutManager = new Android.Support.V7.Widget.LinearLayoutManager(mRecyclerView.Context);
            mRecyclerView.SetLayoutManager(mLayoutManager);
            mAdapter = new PackageAdapter(mRecyclerList);
            mRecyclerView.SetAdapter(mAdapter);
        }

        void CreateData()
        {
            mRecyclerList = new List<PaymentDataModels>();
            mRecyclerList.Add(new PaymentDataModels { TypeText = "Cash", TypeImg = Resource.Drawable.icons8_money_24px_1 });
            mRecyclerList.Add(new PaymentDataModels { TypeText = "Visa Card", TypeImg = Resource.Drawable.ic_visa });
        }

        public void ShowProgressDialog()
        {
            builder = new Android.Support.V7.App.AlertDialog.Builder(this);
            builder.SetView(Resource.Layout.progress_dialog_layout);
            builder.SetCancelable(false);
            alertDialog = builder.Show();
        }

        public void CloseProgressDialog()
        {
            if (alertDialog != null)
            {
                alertDialog.Dismiss();
                alertDialog = null;
                builder = null;
            }
        }

        public override bool OnSupportNavigateUp()
        {
            Finish();
            return true;
        }
    }
}