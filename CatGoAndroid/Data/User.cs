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
using SQLite;

namespace CatGoAndroid
{
    [Table("User")]
    class User
    {
        public string UserName { get; set; }
        public int Points { get; set; }
        public int Level { get; set; }
        public int Items { get; set; }
        public List<Photo> Photos { get; set; }
    }
}