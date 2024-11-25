using SQLite;

namespace RockClimber
{
    public class SavedPath
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Steps { get; set; }
        public string ImagePath { get; set; }  // Path to the saved image
    }
}
