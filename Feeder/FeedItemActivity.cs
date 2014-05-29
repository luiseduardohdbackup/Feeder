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
using Newtonsoft.Json;
using Android.Util;

namespace Feeder
{
    [Activity(Label = "FeedItemActivity")]
    public class FeedItemActivity : Activity
    {
        private RssFeed _feed;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.FeedItemList);

            _feed = JsonConvert.DeserializeObject<RssFeed>(Intent.GetStringExtra("feed"));

            var feedNameTextView = FindViewById<TextView>(Resource.Id.feedItemName);
            var articleCountTextView = FindViewById<TextView>(Resource.Id.articleCount);
            var feedItemListView = FindViewById<ListView>(Resource.Id.feedItemList);

            feedItemListView.ItemClick += feedItemListView_ItemClick;
            feedNameTextView.Text = "Name: " + _feed.Name;
            articleCountTextView.Text = "Articles: " + _feed.Items.Count;

            feedItemListView.Adapter = new FeedItemAdapter(this, _feed.Items.ToArray());

        }

        void feedItemListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var item = _feed.Items[e.Position];

            var uri = Android.Net.Uri.Parse(item.Link);
            var intent = new Intent(Intent.ActionView, uri);
            StartActivity(intent);
        }
    }
}