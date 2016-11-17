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
using Android.Util;
using Android.Content.PM;
using Android.Support.V4.App;
using Android.Support.V4.Content;

namespace CatGoAndroid
{
    public class PermissionUtils
    {
        public static bool RequestPermission(Activity activity, int requestCode, string[] permissions)
        {
            bool granted = true;
            var permissionsNeeded = new List<string>();

            foreach (var s in permissions)
            {
                var permissionCheck = PermissionChecker.CheckSelfPermission(activity, s);
                bool hasPermission = (permissionCheck == (int)Permission.Granted);
                granted &= hasPermission;
                if (!hasPermission)
                {
                    permissionsNeeded.Add(s);
                }
            }

            if (granted)
            {
                return true;
            }
            else
            {
                ActivityCompat.RequestPermissions(activity,
                         permissionsNeeded.ToArray(),
                        requestCode);
                return false;
            }
        }


        public static bool PermissionGranted(int requestCode, int permissionCode, int[] grantResults)
        {
            if (requestCode == permissionCode)
            {
                if (grantResults.Any() && grantResults[0] == (int)Permission.Granted)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
    }

}