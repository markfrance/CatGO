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
using Android.Util;
using System.Threading.Tasks;
using Android.Support.V4.Content;
using Android.Support.V4.App;
using Android;
using Java.Security;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Vision.v1;
using Google.Apis.Vision.v1.Data;
using Google.Apis.Services;
using Android.Support.V7.App;

namespace CatGoAndroid
{
    [Activity(Label = "Camera", MainLauncher = true)]
    class CameraActivity : Activity, ILocationListener
    {
        //Todo
        private string _dbPath = System.IO.Path.Combine(
    System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
    "database.db3");

        static readonly string TAG = "X:" + typeof(CameraActivity).Name;

        LocationManager _locationManager;
        string _locationProvider;
        Location _currentLocation;
        TextView _addressText;
        TextView _locationText;
        TextView _imageDescription;
        ImageView _imageView;

        public static File _file;
        public static File _dir;
        public static Bitmap bitmap;

        public Android.Net.Uri _uri;

        string _description;

        public int TAG_CODE_PERMISSION_LOCATION { get; private set; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Camera);

            _addressText = FindViewById<TextView>(Resource.Id.address_text);
            _locationText = FindViewById<TextView>(Resource.Id.location_text);
            FindViewById<TextView>(Resource.Id.get_address).Click += AddressButton_OnClick;

            InitializeLocationManager();

            if (IsThereAnAppToTakePictures())
            {
                CreateDirectoryForPictures();

                _imageView = FindViewById<ImageView>(Resource.Id.imageView);

                Button button = FindViewById<Button>(Resource.Id.myButton);
                button.Click += TakeAPicture;
            }

            CreateInitialDB();

            var save = FindViewById<Button>(Resource.Id.save);

            save.Click += (object sender, EventArgs e) =>
            {
                if (_file == null)
                    return;

                SaveNewImage();

                var profile = new Intent(this, typeof(ProfileActivity));
                //  StartActivity(profile);
            };


            var delete = FindViewById<Button>(Resource.Id.delete);

            delete.Click += (object sender, EventArgs e) =>
            {
                if (_file == null)
                    return;

                var profile = new Intent(this, typeof(ProfileActivity));
                StartActivity(profile);
            };

            _imageDescription = FindViewById<TextView>(Resource.Id.image_description);

            UpdateCurrentLocation();
        }


        private void CreateInitialDB()
        {
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
            }
        }
        private void UpdateCurrentLocation()
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Android.Content.PM.Permission.Granted)
                _locationManager.RequestLocationUpdates(_locationProvider, 0, 0, this);
            else
                ActivityCompat.RequestPermissions(this, new string[] {
                Manifest.Permission.AccessFineLocation,
                Manifest.Permission.AccessCoarseLocation },
                       1);
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Android.Content.PM.Permission.Granted)
                _locationManager.RequestLocationUpdates(_locationProvider, 0, 0, this);
        }

        protected override void OnPause()
        {
            base.OnPause();
            _locationManager.RemoveUpdates(this);
        }

        async void AddressButton_OnClick(object sender, EventArgs eventArgs)
        {
            if (_currentLocation == null)
            {
                _addressText.Text = "Can't determine the current address. Try again in a few minutes.";
                return;
            }

            Address address = await ReverseGeocodeCurrentLocation();
            DisplayAddress(address);
        }

        async Task<Address> ReverseGeocodeCurrentLocation()
        {
            Geocoder geocoder = new Geocoder(this);
            IList<Address> addressList =
                await geocoder.GetFromLocationAsync(_currentLocation.Latitude, _currentLocation.Longitude, 10);

            Address address = addressList.FirstOrDefault();
            return address;
        }

        void DisplayAddress(Address address)
        {
            if (address != null)
            {
                StringBuilder deviceAddress = new StringBuilder();
                for (int i = 0; i < address.MaxAddressLineIndex; i++)
                {
                    deviceAddress.AppendLine(address.GetAddressLine(i));
                }
                // Remove the last comma from the end of the address.
                _addressText.Text = deviceAddress.ToString();
            }
            else
            {
                _addressText.Text = "Unable to determine the address. Try again in a few minutes.";
            }
        }

        public async void OnLocationChanged(Location location)
        {
            _currentLocation = location;
            if (_currentLocation == null)
            {
                _locationText.Text = "Unable to determine your location. Try again in a short while.";
            }
            else
            {
                _locationText.Text = string.Format("{0:f6},{1:f6}", _currentLocation.Latitude, _currentLocation.Longitude);
                Address address = await ReverseGeocodeCurrentLocation();
                DisplayAddress(address);
            }
        }

        private void InitializeLocationManager()
        {
            _locationManager = (LocationManager)GetSystemService(LocationService);
            Criteria criteriaForLocationService = new Criteria
            {
                Accuracy = Accuracy.Fine
            };
            IList<string> acceptableLocationProviders = _locationManager.GetProviders(criteriaForLocationService, true);

            if (acceptableLocationProviders.Any())
            {
                _locationProvider = acceptableLocationProviders.First();
            }
            else
            {
                _locationProvider = string.Empty;
            }
            Log.Debug(TAG, "Using " + _locationProvider + ".");
        }


        private void SaveNewImage()
        {
            var description = "test123"; // GetValidDescription();

            //if (description == null)
            //{
            //    Toast.MakeText(this, "This doesn't look much like a cat, please try again", ToastLength.Short).Show();
            //    return;
            //}

            UploadImage(_uri);

            UpdateCurrentLocation();

            using (var db = new SQLiteConnection(_dbPath))
            {
                var photo = new Photo
                {
                    Date = DateTime.Now.ToString(),
                    Description = description,
                    Image = _file.AbsolutePath,
                    Name = "Test1",
                    Latitude = 0.000,//_currentLocation.Latitude,
                    Longitude = 0.000//_currentLocation.Longitude
                };

                db.Insert(photo);

                //Update user score
                var user = db.Get<User>(u => u.Name == "Default");

                user.Points += 3;
                user.Level++;

                db.Update(user);

            }

            Toast.MakeText(this, "Image Saved +1 CP: ", ToastLength.Short).Show();
        }


        public void UploadImage(Android.Net.Uri uri)
        {
            if (uri != null)
            {
                try
                { //scale image to save on bandwidth
                    var bitmap = ScaleBitmapDown(MediaStore.Images.Media.GetBitmap(ContentResolver, uri), 1200);

                    CallCloudVision(bitmap);

                }
                catch (Java.IO.IOException e)
                {
                    Toast.MakeText(this, Resource.String.image_picker_error, ToastLength.Long).Show();
                }
            }
            else
            {
                Toast.MakeText(this, Resource.String.image_picker_error, ToastLength.Long).Show();
            }
        }

        private void CallCloudVision(Android.Graphics.Bitmap bitmap)
        {
            _imageDescription.SetText(Resource.String.loading_message);

            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", @"C:\CatGo-46215182ea6e.json");

            try
            {
                GoogleCredential credential = GoogleCredential.GetApplicationDefaultAsync().Result;

                if (credential.IsCreateScopedRequired)
                {
                    credential = credential.CreateScoped(new[]
                    {
                    VisionService.Scope.CloudPlatform
                });
                }

                var vision = new VisionService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    GZipEnabled = false
                });

                var byteArrayOutputStream = new System.IO.MemoryStream();
                bitmap.Compress(Bitmap.CompressFormat.Png, 100, byteArrayOutputStream);
                var byteArray = byteArrayOutputStream.ToArray();

                var imageData = Convert.ToBase64String(byteArray);

                var responses = vision.Images.Annotate(
                    new BatchAnnotateImagesRequest()
                    {
                        Requests = new[] {
                    new AnnotateImageRequest() {
                        Features = new [] { new Feature() { Type =
                          "LABEL_DETECTION"}},
                        Image = new Image() { Content = imageData }
                    }
                   }
                    }).Execute();

                var result = responses.Responses;

                foreach (var test in result)
                {
                    foreach (var label in test.LabelAnnotations)
                    {
                        _description += $"{label.Description} ({label.Score}) .";
                    }
                }
            }
            catch (System.Exception e)
            {
                var test = e.InnerException;
                //needs restart

                var test2 = System.Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process);

            }

            _imageDescription.Text = _description;
        }

        public Bitmap ScaleBitmapDown(Bitmap bitmap, int maxDimension)
        {
            int originalWidth = bitmap.Width;
            int originalHeight = bitmap.Height;
            int resizedWidth = maxDimension;
            int resizedHeight = maxDimension;

            if (originalHeight > originalWidth)
            {
                resizedHeight = maxDimension;
                resizedWidth = (int)(resizedHeight * (float)originalWidth / originalHeight);
            }
            else if (originalWidth > originalHeight)
            {
                resizedWidth = maxDimension;
                resizedHeight = (int)(resizedWidth * (float)originalHeight / originalWidth);
            }
            else if (originalHeight == originalWidth)
            {
                resizedHeight = maxDimension;
                resizedWidth = maxDimension;
            }
            return Bitmap.CreateScaledBitmap(bitmap, resizedWidth, resizedHeight, false);
        }

        private void CreateDirectoryForPictures()
        {
            _dir = new File(
                Android.OS.Environment.GetExternalStoragePublicDirectory(
                    Android.OS.Environment.DirectoryPictures), "CatGo");
            if (!_dir.Exists())
            {
                _dir.Mkdirs();
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
            _file = new File(_dir, string.Format("myPhoto_{0}.jpg", Guid.NewGuid()));


            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) == Android.Content.PM.Permission.Granted &&
                ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) == Android.Content.PM.Permission.Granted &&
                ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) == Android.Content.PM.Permission.Granted)
            {
                _uri = FileProvider.GetUriForFile(ApplicationContext, "com.company.app.fileprovider", _file);
                intent.PutExtra(MediaStore.ExtraOutput, _uri);
                StartActivityForResult(intent, 0);
            }
            else
            {
                ActivityCompat.RequestPermissions(this, new string[] {
                Manifest.Permission.ReadExternalStorage,
                Manifest.Permission.WriteExternalStorage, Manifest.Permission.Camera },
                       1);
                Toast.MakeText(this, "Please try again", ToastLength.Short).Show();
                return;
            }

        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            // Make it available in the gallery

            var mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
            var contentUri = Android.Net.Uri.FromFile(_file);
            mediaScanIntent.SetData(contentUri);
            SendBroadcast(mediaScanIntent);

            // Display in ImageView. We will resize the bitmap to fit the display.
            // Loading the full sized image will consume to much memory
            // and cause the application to crash.

            int height = Resources.DisplayMetrics.HeightPixels;
            int width = _imageView.Height;
            bitmap = _file.Path.LoadAndResizeBitmap(width, height);
            if (bitmap != null)
            {
                _imageView.SetImageBitmap(bitmap);
                bitmap = null;
            }

            // Dispose of the Java side bitmap.
            GC.Collect();
        }

        #region ILocationListener

        public void OnProviderDisabled(string provider) { }

        public void OnProviderEnabled(string provider) { }

        public void OnStatusChanged(string provider, Availability status, Bundle extras) { }
        #endregion

    }
}