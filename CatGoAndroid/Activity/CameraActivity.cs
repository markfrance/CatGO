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

using Android.Graphics;
using Java.IO;
using Android.Provider;
using Android.Content.PM;
using Android.Net;
using Android.Locations;
using SQLite;

namespace CatGoAndroid
{
    public static class App
    {
        public static File _file;
        public static File _dir;
        public static Bitmap bitmap;
    }

    [Activity(Label = "Camera")]
    class CameraActivity : Activity
    {
        //Todo
        private string _dbPath = System.IO.Path.Combine(
    System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
    "database.db3");

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Camera);

            if (IsThereAnAppToTakePictures())
            {
                CreateDirectoryForPictures();

                Button button = FindViewById<Button>(Resource.Id.myButton);
                var _imageView = FindViewById<ImageView>(Resource.Id.imageView1);
                button.Click += TakeAPicture;
            }

            var save = FindViewById<Button>(Resource.Id.save);

            save.Click += (object sender, EventArgs e) =>
            {
                SaveNewImage();
                Toast.MakeText(this, "Image Saved +1 CP: ",
                      ToastLength.Short).Show();

                var profile = new Intent(this, typeof(ProfileActivity));
                StartActivity(profile);
            };


            var delete = FindViewById<Button>(Resource.Id.delete);

            delete.Click += (object sender, EventArgs e) =>
            {
                var profile = new Intent(this, typeof(ProfileActivity));
                StartActivity(profile);
            };
        }

        private void SaveNewImage()
        {
            using (var db = new SQLiteConnection(_dbPath))
            {
                var photo = new Photo
                {
                    Date = DateTime.Now.ToString(),
                    Description = "Cat",
                    Image = "Cat3.jpg",
                    Name = "Test1",
                    Latitude = 51.56590,
                    Longitude = -0.134427
                };
                db.Insert(photo);


                //Update user score
                var user = db.Get<User>(u => u.Name == "Default");

                user.Points += 3;
                user.Level++;

                db.Update(user);

            }
        }

        private void CreateDirectoryForPictures()
        {
            App._dir = new File(
                Android.OS.Environment.GetExternalStoragePublicDirectory(
                    Android.OS.Environment.DirectoryPictures), "CatGo");
            if (!App._dir.Exists())
            {
                App._dir.Mkdirs();
            }
        }

        private bool IsThereAnAppToTakePictures()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            IList<ResolveInfo> availableActivities =
                PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }

        private void TakeAPicture(object sender, EventArgs eventArgs)
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            App._file = new File(App._dir, String.Format("myPhoto_{0}.jpg", Guid.NewGuid()));
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(App._file));
            StartActivityForResult(intent, 0);
        }
    }
}