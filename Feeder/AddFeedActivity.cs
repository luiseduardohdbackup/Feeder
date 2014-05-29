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

namespace Feeder
{
    [Activity(Label = "AddFeedActivity")]
    public class AddFeedActivity : Activity
    {
        private EditText _editTextView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.AddFeed);

            _editTextView = FindViewById<EditText>(Resource.Id.feedUrl);
            var addButton = FindViewById<Button>(Resource.Id.addBtn);

            addButton.Click += addButton_Click;
        }

        void addButton_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(FeedListActivity));
            intent.PutExtra("url", _editTextView.Text);
            SetResult(Result.Ok, intent);
            Finish();
        }
    }
}