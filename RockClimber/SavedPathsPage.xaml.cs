using RockClimber;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using System.IO;

namespace RockClimber
{
    public partial class SavedPathsPage : ContentPage
    {
        private DatabaseHelper _databaseHelper;

        public SavedPathsPage()
        {
            InitializeComponent();

            // Get the database path
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "savedPaths.db3");

            // Initialize the database helper with the dbPath
            _databaseHelper = new DatabaseHelper(dbPath);

            // Load saved paths
            LoadSavedPaths();
        }

        // Load saved paths from the database and bind to the ListView
        private void LoadSavedPaths()
        {
            // Get all saved paths from the database
            var savedPaths = _databaseHelper.GetSavedPaths();

            // Bind the retrieved paths to the ListView
            SavedPathsListView.ItemsSource = savedPaths;
        }

        // Handle item selection in the ListView
        private async void OnPathSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var selectedPath = e.SelectedItem as SavedPath;

            if (selectedPath != null)
            {
                // For now, just show a simple alert with the steps and image path
                await DisplayAlert("Path Selected",
                    $"Steps: {selectedPath.Steps}\nImage Path: {selectedPath.ImagePath}",
                    "OK");

            }
        }
    }
}
