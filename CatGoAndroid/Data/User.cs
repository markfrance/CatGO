using SQLite;

namespace CatGoAndroid
{
    [Table("User")]
    class User
    {
        [PrimaryKey]
        public string Name { get; set; }
        public int Points { get; set; }
        public int Level { get; set; }
        public int Items { get; set; }
    }
}