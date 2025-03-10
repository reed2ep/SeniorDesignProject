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

            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "savedPaths.db3");
            _databaseHelper = new DatabaseHelper(dbPath);

            LoadSavedPaths();
        }

        private void LoadSavedPaths()
        {
            var savedPaths = _databaseHelper.GetSavedPaths(); // Fetch saved paths from DB
            SavedPathsListView.ItemsSource = savedPaths;
        }

        private async void OnPathSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var selectedPath = e.SelectedItem as SavedPath;
            if (selectedPath == null) return;

            // Retrieve route moves as a List<Move>
            var routeMoves = _databaseHelper.GetRouteMovesById(selectedPath.Id);
            var imagePath = selectedPath.ImagePath;

            // Navigate to PathDisplayPage
            await Navigation.PushAsync(new PathDisplayPage(routeMoves, imagePath));
        }
        private async void OnDeletePathClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var selectedPath = button?.CommandParameter as SavedPath;

            if (selectedPath == null) return;

            bool confirm = await DisplayAlert("Delete", $"Are you sure you want to delete '{selectedPath.Name}'?", "Yes", "No");
            if (confirm)
            {
                _databaseHelper.DeletePath(selectedPath.Id);
                LoadSavedPaths(); // Refresh the list
            }
        }
    }
}
