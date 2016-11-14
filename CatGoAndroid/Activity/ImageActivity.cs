using Android;
using Android.App;
using Android.OS;
using Android.Webkit;
using Android.Widget;

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

        }
    }
}