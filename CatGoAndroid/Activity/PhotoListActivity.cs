using Android.App;
using Android.Widget;
using Android.OS;
using System.Xml.Linq;
using System.Net.Http;
using System.Linq;
using Android.Content;
using Android.Views;
using SQLite;
using System.IO;
using System;

namespace CatGoAndroid
{
    [Activity(Label = "Test", Icon = "@drawable/icon")]
    public class PhotoListActivity : ListActivity
    {
        private Photo[] _photos;
        private string _dbPath = Path.Combine(
        System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
        "database.db3");

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            using (var db = new SQLiteConnection(_dbPath))
            {
                db.CreateTable<Photo>();

                if (db.Table<Photo>().Count() == 0)
                {
                    var photo1 = new Photo
                    {
                        Date = DateTime.Now.ToString(),
                        Description = "Cat",
                        Image = "Cat.jpg",
                        Name = "Test1",
                        Latitude = 51.501690,
                        Longitude = -0.1263427
                    };
                    db.Insert(photo1);

                    var photo2 = new Photo
                    {
                        Date = DateTime.Now.ToString(),
                        Description = "Cat2",
                        Image = "Cat2.jpg",
                        Name = "Test2",
                        Latitude = 51.501690,
                        Longitude = -0.1263427
                    };
                    db.Insert(photo2);
                }

                var photoList = db.Table<Photo>();

                _photos = photoList.ToArray();

                ListAdapter = new PhotoFeedAdapter(this, _photos);
            }

        }

        protected override void OnListItemClick(ListView list, View view, int position, long id)
        {
            base.OnListItemClick(list, view, position, id);

            var second = new Intent(this, typeof(ImageActivity));
            second.PutExtra("link", _photos[position].Image);
            StartActivity(second);
        }
    }


}

