using SQLite;

namespace RockClimber
{
    public class SavedPath
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; } // Path name
        public string Steps { get; set; } // Route moves
        public string ImagePath { get; set; } // Path to the input image
    }
}
