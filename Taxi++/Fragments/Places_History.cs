using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Taxi__.Adapters;
using Taxi__.DataModels;
using Taxi__.EventListeners;
using Taxi__.Helpers;
using static Android.Support.V7.Widget.RecyclerView;

namespace Taxi__.Fragments
{
    public class Places_History : Android.Support.V4.App.Fragment
    {
        RecentHistoryListener historyListener;
        RecyclerView history_recycler;
        //Recent Search
        List<NewTripDetails> recent_searches;
        HistoryAdapter historyAdapter;

        private Android.Support.V7.Widget.Toolbar myToolbar;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.places_layout, container, false);
            myToolbar = (Android.Support.V7.Widget.Toolbar)view.FindViewById(Resource.Id.places_toolbar);
            myToolbar.Title = "History";

            RetrieveData();
            history_recycler = (RecyclerView)view.FindViewById(Resource.Id.history_recycler);
            return view;
        }

        public void RetrieveData()
        {
            historyListener = new RecentHistoryListener();
            historyListener.Create();
            historyListener.HistoryRetrieved += HistoryListener_HistoryRetrieved;
        }

        private void HistoryListener_HistoryRetrieved(object sender, RecentHistoryListener.RecentTripEventArgs e)
        {
            recent_searches = e.RecentTripList;
            SetUpRecyclerView();
        }

        private void SetUpRecyclerView()
        {
            LayoutManager layoutManager = new LinearLayoutManager(history_recycler.Context);
            historyAdapter = new HistoryAdapter(recent_searches);
            history_recycler.SetAdapter(historyAdapter);
            history_recycler.SetLayoutManager(layoutManager);

            ItemDecoration itemDecoration = new RecyclerItemDecor(Application.Context.GetDrawable(Resource.Drawable.divider));
            history_recycler.AddItemDecoration(itemDecoration);
            historyAdapter.ItemClick += HistoryAdapter_ItemClick;
        }

        private void HistoryAdapter_ItemClick(object sender, int e)
        {
            Toast.MakeText(Application.Context, "Hello world", ToastLength.Short).Show();
        }
    }
}