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
using Android;
using Android.Provider;
using Android.Net;
using Java.IO;
using Android.Content.PM;
using Android.Graphics;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Vision.v1;
using Google.Apis.Vision.v1.Data;
using Google.Apis.Http;
using Google.Apis.Json;
using Android.Support.V7.App;
using Android.Net.Http;
using Android.Util;
using System.IO;

namespace CatGoAndroid
{
    [Activity(Label = "LabelDetectionActivity", MainLauncher = true)]
    public class LabelDetectionActivity : AppCompatActivity
    {
        private static readonly string CLOUD_VISION_API_KEY = "";
        public static string FILE_NAME = "test.jpg";

        private static readonly string TAG = typeof(LabelDetectionActivity).Name;
        private static readonly int GALLERY_IMAGE_REQUEST = 1;
        private static readonly int CAMERA_PERMISSIONS_REQUEST = 2;
        public static readonly int CAMERA_IMAGE_REQUEST = 3;

        private TextView mImageDetails;
        private ImageView mMainImage;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_main);
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            var fab = FindViewById<Button>(Resource.Id.fab);

            fab.Click += Fab_Click;


            mImageDetails = FindViewById<TextView>(Resource.Id.image_details);
            mMainImage = FindViewById<ImageView>(Resource.Id.main_image);
        }

        private void Fab_Click(object sender, EventArgs e)
        {

            var builder = new Android.App.AlertDialog.Builder(this.ApplicationContext);

            //  builder.SetMessage(Resource.String.dialog_select_prompt)
            //    .SetPositiveButton(Resource.String.dialog_select_gallery,)
        }

        public void StartGalleryChooser()
        {
            var intent = new Intent();
            intent.SetType("image/*");
            intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(intent, "Select a photo"), GALLERY_IMAGE_REQUEST);
        }

        public void StartCamera()
        {
            if (PermissionUtils.RequestPermission(this, CAMERA_PERMISSIONS_REQUEST,
                new string[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.Camera }))
            {
                var intent = new Intent(MediaStore.ActionImageCapture);
                intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(GetCameraFile()));
                StartActivityForResult(intent, CAMERA_IMAGE_REQUEST);
            }
        }

        public Java.IO.File GetCameraFile()
        {
            var dir = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures);
            return new Java.IO.File(dir, FILE_NAME);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == GALLERY_IMAGE_REQUEST && resultCode == Result.Ok && data != null)
            {
                UploadImage(data.Data);
            }
            else if (requestCode == CAMERA_IMAGE_REQUEST && resultCode == Result.Ok)
            {
                UploadImage(Android.Net.Uri.FromFile(GetCameraFile()));
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (PermissionUtils.PermissionGranted(requestCode, CAMERA_PERMISSIONS_REQUEST, Array.ConvertAll(grantResults, val => (int)val)))
                StartCamera();
        }

        public void UploadImage(Android.Net.Uri uri)
        {
            if (uri != null)
            {
                try
                { //scale image to save on bandwidth
                    var bitmap = ScaleBitmapDown(MediaStore.Images.Media.GetBitmap(ContentResolver, uri), 1200);

                    CallCloudVision(bitmap);
                    mMainImage.SetImageBitmap(bitmap);
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

        //TODO: ASync
        private void CallCloudVision(Android.Graphics.Bitmap bitmap)
        {
            mImageDetails.SetText(Resource.String.loading_message);

            GoogleCredential credential =
                GoogleCredential.GetApplicationDefaultAsync().Result;

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

            var byteArrayOutputStream = new MemoryStream();
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

        private string convertResponseToString(BatchAnnotateImagesResponse response)
        {
            var message = "I found these things:\n\n";

            List<EntityAnnotation> labels = response.Responses.First().LabelAnnotations.ToList();
            if (labels != null)
            {
                foreach (var label in labels)
                {
                    message += string.Format("%.3f: %s", label.Score, label.Description);
                    message += "\n";
                }
            }
            else
            {
                message += "nothing";
            }

            return message;
        }

    }
}