using SQLite;
using System.Collections.Generic;
using System.IO;

namespace RockClimber
{
    public class DatabaseHelper
    {
        private SQLiteConnection _database;

        public DatabaseHelper(string dbPath)
        {
            // Open the SQLite database connection
            _database = new SQLiteConnection(dbPath);
            // Create the SavedPath table if it doesn't exist
            _database.CreateTable<SavedPath>();
        }

        // Save a new saved path to the database
        public void SavePath(SavedPath savedPath)
        {
            _database.Insert(savedPath);
        }

        // Retrieve all saved paths from the database
        public List<SavedPath> GetSavedPaths()
        {
            return _database.Table<SavedPath>().ToList();
        }

        // Optionally, get a saved path by ID (if needed)
        public SavedPath GetSavedPathById(int id)
        {
            return _database.Table<SavedPath>().FirstOrDefault(sp => sp.Id == id);
        }
    }
}
