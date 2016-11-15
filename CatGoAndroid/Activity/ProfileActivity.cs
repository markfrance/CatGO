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
using SQLite;

namespace CatGoAndroid
{
    [Activity(Label = "ProfileActivity", MainLauncher = true)]
    public class ProfileActivity : Activity
    {

        //TODO:
        private string _dbPath = Path.Combine(
        System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
        "database.db3");

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Profile);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetActionBar(toolbar);
            ActionBar.Title = "Menu";


            //Create user
            using (var db = new SQLiteConnection(_dbPath))
            {
                db.CreateTable<User>();

                if (db.Table<User>().Count() == 0)
                {
                    db.Insert(new User
                    {
                        Name = "Default",
                        Items = 0,
                        Points = 0,
                        Level = 1
                    });
                }

                var user = db.Get<User>(u => u.Name == "Default");

                var name = FindViewById<TextView>(Resource.Id.profile_name);
                name.Text = $"Name: {user.Name}";

                var points = FindViewById<TextView>(Resource.Id.profile_cp);
                points.Text = $"Points: {user.Points}";

                var level = FindViewById<TextView>(Resource.Id.profile_level);
                level.Text = $"Level: {user.Level}";

                var items = FindViewById<TextView>(Resource.Id.profile_items);
                items.Text = $"Items: {user.Items}";

            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.top_menus, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Intent activity = null;

            switch (item.ItemId)
            {
                case Resource.Id.menu_profile:
                    activity = new Intent(this, typeof(ProfileActivity));
                    break;
                case Resource.Id.menu_camera:
                    activity = new Intent(this, typeof(CameraActivity));
                    break;
                case Resource.Id.menu_list:
                    activity = new Intent(this, typeof(PhotoListActivity));
                    break;
                case Resource.Id.menu_map:
                    var geoUri = Android.Net.Uri.Parse("geo:42.374260,-71.120824");
                    activity = new Intent(Intent.ActionView, geoUri);
                    break;
                default:
                    break;

            }

            StartActivity(activity);
            return base.OnOptionsItemSelected(item);
        }
    }
}