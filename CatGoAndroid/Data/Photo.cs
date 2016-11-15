using SQLite;

namespace CatGoAndroid
{
    [Table("Photos")]
    public class Photo
    {
        [PrimaryKey]
        public string Name { get; set; }
        public string Date { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

    }
}