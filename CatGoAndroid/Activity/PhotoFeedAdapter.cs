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
    class PhotoFeedAdapter : BaseAdapter<Photo>
    {
        private Photo[] _items;
        private Activity _context;

        public PhotoFeedAdapter(Activity context, Photo[] items) : base()
        {
            _context = context;
            _items = items;
        }

        public override Photo this[int position]
        {
            get { return _items[position]; }
        }

        public override int Count
        {
            get { return _items.Count(); }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;
            if (view == null)
            {
                view = _context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItem2, null);
            }

            view.FindViewById<TextView>(Android.Resource.Id.Text1).Text = _items[position].Name;
            view.FindViewById<TextView>(Android.Resource.Id.Text2).Text = string.Format("{0} on {1}", _items[position].Description, _items[position].Date);

            

            return view;
        }
    }
}