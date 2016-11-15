using System;
using Android;
using Android.App;
using Android.OS;
using Android.Webkit;
using Android.Widget;
using Android.Content;
using System.IO;
using SQLite;

namespace CatGoAndroid
{
    [Activity(Label = "View Photo")]
    class ImageActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.ImageView);

            var view = FindViewById<ImageView>(Resource.Id.photoView);

            view.SetImageResource(Resource.Drawable.Icon);

            var edit = FindViewById<Button>(Resource.Id.edit_image);

            edit.Click += Edit_Click;

        }

        private void Edit_Click(object sender, EventArgs e)
        {
            Toast.MakeText(this, "Edit Image: TODO ",
                      ToastLength.Short).Show();
        }
    }
}