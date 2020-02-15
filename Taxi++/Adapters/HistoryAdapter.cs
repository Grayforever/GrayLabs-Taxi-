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
        public event EventHandler<int> ItemClick;

        List<NewTripDetails> search_list;

        public HistoryAdapter(List<NewTripDetails> data)
        {
            search_list = data;
            NotifyDataSetChanged();
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.history_adapterview, parent, false);
            var vh = new HistoryAdapterViewHolder(itemView, OnClick);
            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var item = search_list[position];

            // Replace the contents of the view with that element
            var holder = viewHolder as HistoryAdapterViewHolder;
            holder.PlaceTextView.Text = search_list[position].DestinationAddress;
        }

        public override int ItemCount => search_list.Count;

        void OnClick(int position) => ItemClick?.Invoke(this, position);

    }

    public class HistoryAdapterViewHolder : RecyclerView.ViewHolder
    {
        public TextView PlaceTextView { get; set; }

        public HistoryAdapterViewHolder(View itemView, Action<int> clickListener) : base(itemView)
        {
            PlaceTextView = (TextView)itemView.FindViewById (Resource.Id.history_tv);
            
            itemView.Click += (sender, pos) => clickListener(LayoutPosition);
        }
    }
}