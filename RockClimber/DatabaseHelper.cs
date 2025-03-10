using SQLite;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace RockClimber
{
    public class DatabaseHelper
    {
        private SQLiteConnection _database;
        private readonly string _dbPath;

        public DatabaseHelper(string dbPath)
        {
            // Open the SQLite database connection
            _database = new SQLiteConnection(dbPath);
            // Create the SavedPath table if it doesn't exist
            _database.CreateTable<SavedPath>();
        }

        // Save a new saved path to the database
        public void SavePath(string name, List<Move> routeMoves, string imagePath)
        {
            string movesJson = JsonConvert.SerializeObject(routeMoves);

            var savedPath = new SavedPath
            {
                Name = name,
                Steps = movesJson,
                ImagePath = imagePath
            };

            _database.Insert(savedPath);
        }

        public void DeletePath(int id)
        {
            var savedPath = _database.Table<SavedPath>().FirstOrDefault(p => p.Id == id);
            if (savedPath != null)
            {
                _database.Delete(savedPath);
            }
        }

        // Retrieve all saved paths from the database
        public List<SavedPath> GetSavedPaths()
        {
            return _database.Table<SavedPath>().ToList();
        }

        // Get a saved path by ID
        public (string RouteMoves, string ImagePath) GetSavedPathById(int id)
        {
            var savedPath = _database.Table<SavedPath>().FirstOrDefault(p => p.Id == id);
            if (savedPath != null)
            {
                return (savedPath.Steps, savedPath.ImagePath);
            }
            return (string.Empty, string.Empty);
        }

        public List<Move> GetRouteMovesById(int id)
        {
            var savedPath = _database.Table<SavedPath>().FirstOrDefault(p => p.Id == id);
            if (savedPath == null || string.IsNullOrEmpty(savedPath.Steps))
                return new List<Move>();

            return JsonConvert.DeserializeObject<List<Move>>(savedPath.Steps);
        }
    }
}
