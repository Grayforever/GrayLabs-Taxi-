using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using Taxi__.DataModels;

namespace Taxi__.Adapters
{
    class HistoryAdapter : RecyclerView.Adapter
    {
        public event EventHandler<HistoryAdapterClickEventArgs> ItemClick;
        public event EventHandler<HistoryAdapterClickEventArgs> ItemLongClick;

        List<SearchHistoryModel> search_list;

        public HistoryAdapter(List<SearchHistoryModel> data)
        {
            search_list = data;
            NotifyDataSetChanged();
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.history_adapterview, parent, false);

            var vh = new HistoryAdapterViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var item = search_list[position];

            // Replace the contents of the view with that element
            var holder = viewHolder as HistoryAdapterViewHolder;
            holder.PlaceTextView.Text = search_list[position].PlaceName;
        }

        public override int ItemCount => search_list.Count;

        void OnClick(HistoryAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(HistoryAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class HistoryAdapterViewHolder : RecyclerView.ViewHolder
    {
        public TextView PlaceTextView { get; set; }
        //public LinearLayout linearLayout { get; set; }

        public HistoryAdapterViewHolder(View itemView, Action<HistoryAdapterClickEventArgs> clickListener,
                            Action<HistoryAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            PlaceTextView = (TextView)itemView.FindViewById (Resource.Id.history_tv);
            //linearLayout = (LinearLayout)itemView.FindViewById(Resource.Id.linear_click);
            itemView.Click += (sender, e) => clickListener(new HistoryAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new HistoryAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class HistoryAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}