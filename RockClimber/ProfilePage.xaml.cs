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

            DisplayAlert("Success", "Profile data saved successfully!", "OK");
        }

    }
}
