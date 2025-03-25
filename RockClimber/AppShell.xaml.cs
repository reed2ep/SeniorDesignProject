using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace RockClimber
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        private async void OnHelpClicked(object sender, EventArgs e)
        {
            string url = "https://github.com/reed2ep/SeniorDesignProject"; // Replace with your actual help page URL
            await Launcher.OpenAsync(new Uri(url));
        }
    }
}
