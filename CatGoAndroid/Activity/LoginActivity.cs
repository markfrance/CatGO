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

namespace CatGoAndroid
{
    [Activity(Label = "Login", Theme = "@style/android:Theme.Holo.Light.NoActionBar")]
    public class LoginActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Login);

            var login = FindViewById<Button>(Resource.Id.login);

            login.Click += (object sender, EventArgs e) =>
            {
                var geoUri = Android.Net.Uri.Parse("geo:42.374260,-71.120824");
                var mapIntent = new Intent(Intent.ActionView, geoUri);
                StartActivity(mapIntent);
            };
        }
    }
}
