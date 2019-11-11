using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Stripe.Android;
using Com.Stripe.Android.View;
using Taxi__.Helpers;

namespace Taxi__.Fragments
{
    public class AddPaymentMethod : Android.Support.V4.App.DialogFragment
    {
        Android.Support.V7.Widget.Toolbar mToolbar;
        
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetStyle(StyleNormal, Resource.Style.FullScreenDialogTheme);
            // Create your fragment here
        }

        public static AddPaymentMethod display(Android.Support.V4.App.FragmentManager fragmentManager)
        {
            AddPaymentMethod addPaymentMethod = new AddPaymentMethod();
            addPaymentMethod.Show(fragmentManager, "TAG");
            return addPaymentMethod;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.payments_layout, container, false);
            var mCardInputWidget = (CardInputWidget)view.FindViewById(Resource.Id.stripe_input);
            mToolbar = (Android.Support.V7.Widget.Toolbar)view.FindViewById(Resource.Id.payments_toolbar);
            mToolbar.Title = "Add new card";

            var mVerifyBtn = (Button)view.FindViewById(Resource.Id.add_card_btn);
            mVerifyBtn.Click += (s1, e1) =>
            {
                Com.Stripe.Android.Model.Card card = mCardInputWidget.Card;
                if (card == null)
                {
                    return;
                }
                else
                {
                    Stripe stripe = null;
                    stripe = new Stripe(Application.Context, "pk_test_luVWCRXdHQLbt3pNI1hrAbqU002tJmy1WP");
                    stripe.CreateToken(card, new StripeTokenCallback(Application.Context));
                }
            };
            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            mToolbar.NavigationClick += MToolbar_NavigationClick;
        }

        private void MToolbar_NavigationClick(object sender, Android.Support.V7.Widget.Toolbar.NavigationClickEventArgs e)
        {
            mToolbar.Title = "Add new card";
            mToolbar.InflateMenu(Resource.Menu.sign_out_menu);
        }
    }
}