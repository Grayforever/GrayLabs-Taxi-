using System;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using System.Collections.Generic;
using Android.Graphics;
using Android.App;
using Java.Lang;
using System.Threading.Tasks;
using Taxi__.DataModels;
using Android.Support.Design.Card;

namespace Taxi__.Adapters
{
    class PackageAdapter : RecyclerView.Adapter
    {
        public event EventHandler<PackageAdapterClickEventArgs> ItemClick;
        public event EventHandler<PackageAdapterClickEventArgs> ItemLongClick;
        List<PaymentDataModels> Items;

        public PackageAdapter(List<PaymentDataModels> Data)
        {
            Items = Data;
            NotifyDataSetChanged();
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.payment_card, parent, false);

            var vh = new PackageAdapterViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var holder = viewHolder as PackageAdapterViewHolder;
            holder.TypeText.Text = Items[position].TypeText;
            holder.TypeImg.SetImageResource(Items[position].TypeImg);

        }

        public override int ItemCount =>  Items.Count;

        void OnClick(PackageAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(PackageAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class PackageAdapterViewHolder : RecyclerView.ViewHolder
    {
        //public TextView TextView { get; set; }
        public TextView TypeText { get; set; }
        public ImageView TypeImg { get; set; }
        public MaterialCardView materialcardview { get; set; }

        public PackageAdapterViewHolder(View itemView, Action<PackageAdapterClickEventArgs> clickListener,
                            Action<PackageAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            //TextView = v;
            TypeText = (TextView)itemView.FindViewById(Resource.Id.pay_type_text);
            TypeImg = (ImageView)itemView.FindViewById(Resource.Id.pay_type_img);
            materialcardview = (MaterialCardView)itemView.FindViewById(Resource.Id.card_1);
            materialcardview.Click += (s1, e1) =>
             {
                 Toast.MakeText(Application.Context, "item clicked", ToastLength.Long).Show();
             };

            itemView.Click += (sender, e) => clickListener(new PackageAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new PackageAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class PackageAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}