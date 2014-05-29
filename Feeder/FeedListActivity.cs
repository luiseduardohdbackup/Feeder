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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Android.Util;
using System.Net.Http;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace Feeder
{
    [Activity(Label = "Feeder", MainLauncher = true, Icon = "@drawable/icon")]
    public class FeedListActivity : Activity
    {
        private List<RssFeed> _feeds;
        private ListView _feedList;
        private Button _addFeedButton;
        private const string FEED_FILE_NAME = "FeedData.bin";
        private string _filePath;

        public FeedListActivity()
        {
            _feeds = new List<RssFeed>();
            var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            _filePath = Path.Combine(path, FEED_FILE_NAME);
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.FeedList);

            _feedList = FindViewById<ListView>(Resource.Id.feedList);
            _addFeedButton = FindViewById<Button>(Resource.Id.addFeed);

            _feedList.ItemClick += _feedList_ItemClick;
            _addFeedButton.Click += _addFeedButton_Click;

            if (File.Exists(_filePath))
            {
                using (var fs = new FileStream(_filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    var formatter = new BinaryFormatter();

                    try
                    {
                        _feeds = (List<RssFeed>)formatter.Deserialize(fs);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Feeder", "Encountered an error attempting to deserialize feed data: {0}", ex.Message);
                    }

                    if (_feeds.Count > 0)
                        UpdateList();
                }
            }
        }

        void _addFeedButton_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(AddFeedActivity));

            StartActivityForResult(intent, 0);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                var url = data.GetStringExtra("url");
                AddFeedUrl(url);
            }
        }

        private async void AddFeedUrl(string url)
        {
            var newFeed = new RssFeed
            {
                DateAdded = DateTime.Now,
                Url = url
            };

            using (var client = new HttpClient())
            {
                var xmlFeed = await client.GetStringAsync(url);
                var doc = XDocument.Parse(xmlFeed);

                var channel = doc.Descendants("channel").FirstOrDefault().Element("title").Value;
                newFeed.Name = channel;
                XNamespace dc = "http://purl.org/dc/elements/1.1/";
                newFeed.Items = (from item in doc.Descendants("item")
                                 select new RssItem
                                 {
                                     Title = item.Element("title").Value,
                                     PubDate = item.Element("pubDate").Value,
                                     Creator = item.Element(dc + "creator").Value,
                                     Link = item.Element("link").Value
                                 }).ToList();

                _feeds.Add(newFeed);
                UpdateList();
            }
        }

        private void UpdateList()
        {
            _feedList.Adapter = new FeedListAdapter(_feeds.ToArray(), this);
        }

        void _feedList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var feedItemIntent = new Intent(this, typeof(FeedItemActivity));
            var selectedFeed = _feeds[e.Position];
            var feed = JsonConvert.SerializeObject(selectedFeed);
            feedItemIntent.PutExtra("feed", feed);

            StartActivity(feedItemIntent);
        }

        protected override void OnPause()
        {
            base.OnPause();

            using (var fs = new FileStream(_filePath, FileMode.Create, FileAccess.Write))
            {
                var formatter = new BinaryFormatter();

                try
                {
                    formatter.Serialize(fs, _feeds);
                }
                catch (Exception ex)
                {
                    Log.Error("Feeder", "Encountered an error attempting to serialize feed data: {0}", ex.Message);
                }
            }
        }
    }
}