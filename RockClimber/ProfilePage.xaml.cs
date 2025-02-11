using Microsoft.Maui.Storage;

namespace RockClimber
{
    public partial class ProfilePage : ContentPage
    {
        public ProfilePage()
        {
            InitializeComponent();
            InitializePickers();
            LoadUserData();
        }

        private void InitializePickers()
        {
            // Populate Feet Picker options (for height and wingspan)
            for (int i = 3; i <= 7; i++)
            {
                HeightFeetPicker.Items.Add(i.ToString());
                WingspanFeetPicker.Items.Add(i.ToString());
            }

            // Populate Inches Picker options (for height and wingspan)
            for (int i = 0; i < 12; i++)
            {
                HeightInchesPicker.Items.Add(i.ToString());
                WingspanInchesPicker.Items.Add(i.ToString());
            }

            // Populate Hold Type Ranking options (1-3)
            for (int i = 1; i <= 3; i++)
            {
                JugRankPicker.Items.Add(i.ToString());
                CrimpRankPicker.Items.Add(i.ToString());
                SloperRankPicker.Items.Add(i.ToString());
            }
        }
        private void LoadUserData()
        {
            // Load data from preferences
            NameEntry.Text = Preferences.Get("name", string.Empty);

            // Height
            HeightFeetPicker.SelectedItem = Preferences.Get("heightFeet", 5).ToString();
            HeightInchesPicker.SelectedItem = Preferences.Get("heightInches", 0).ToString();

            // Wingspan
            WingspanFeetPicker.SelectedItem = Preferences.Get("wingspanFeet", 5).ToString();
            WingspanInchesPicker.SelectedItem = Preferences.Get("wingspanInches", 0).ToString();

            // Difficulty Level
            DifficultyPicker.SelectedItem = Preferences.Get("difficulty", "Beginner");

            // Hold Type Rankings
            JugRankPicker.SelectedItem = Preferences.Get("jugRank", 1).ToString();
            CrimpRankPicker.SelectedItem = Preferences.Get("crimpRank", 2).ToString();
            SloperRankPicker.SelectedItem = Preferences.Get("sloperRank", 3).ToString();
        }

        private void OnSaveButtonClicked(object sender, EventArgs e)
        {
            // Save data to preferences
            Preferences.Set("name", NameEntry.Text);

            // Save height values
            Preferences.Set("heightFeet", int.Parse(HeightFeetPicker.SelectedItem?.ToString() ?? "0"));
            Preferences.Set("heightInches", int.Parse(HeightInchesPicker.SelectedItem?.ToString() ?? "0"));

            // Save wingspan values
            Preferences.Set("wingspanFeet", int.Parse(WingspanFeetPicker.SelectedItem?.ToString() ?? "0"));
            Preferences.Set("wingspanInches", int.Parse(WingspanInchesPicker.SelectedItem?.ToString() ?? "0"));

            // Save difficulty level
            Preferences.Set("difficulty", DifficultyPicker.SelectedItem?.ToString() ?? "Beginner");

            // Save Hold Type Rankings
            Preferences.Set("jugRank", int.Parse(JugRankPicker.SelectedItem?.ToString() ?? "1"));
            Preferences.Set("crimpRank", int.Parse(CrimpRankPicker.SelectedItem?.ToString() ?? "2"));
            Preferences.Set("sloperRank", int.Parse(SloperRankPicker.SelectedItem?.ToString() ?? "3"));

            DisplayAlert("Success", "Profile data saved successfully!", "OK");
        }

    }
}
