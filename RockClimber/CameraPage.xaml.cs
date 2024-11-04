using CommunityToolkit.Maui.Media;
using Microsoft.Maui.Controls;
using System;

namespace RockClimber
{
    public partial class CameraPage : ContentPage
    {
        public CameraPage()
        {
            InitializeComponent();
        }

        private async void OnCapturePhotoClicked(object sender, EventArgs e)
        {
            try
            {
                var photo = await MediaPicker.Default.CapturePhotoAsync();

                if (photo != null)
                {
                    using var stream = await photo.OpenReadAsync();
                    CapturedImage.Source = ImageSource.FromStream(() => stream);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Unable to capture photo: {ex.Message}", "OK");
            }
        }
    }
}
