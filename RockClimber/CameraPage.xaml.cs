using CommunityToolkit.Maui.Media;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using Android.Graphics; // For Android.Graphics.Bitmap
using Microsoft.Maui.Dispatching;


namespace RockClimber
{
    public partial class CameraPage : ContentPage
    {
        private string _imagePath = string.Empty;
        private MCvScalar _lowerBound;
        private MCvScalar _upperBound;

        public CameraPage()
        {
            InitializeComponent();
        }

        public CameraPage(string imagePath) : this()
        {
            _imagePath = imagePath;

            // Display the image in the UI
            CapturedImage.Source = ImageSource.FromFile(imagePath);
        }

        private async void OnCapturePhotoClicked(object sender, EventArgs e)
        {
            try
            {
                // Capture photo using MediaPicker
                var photo = await MediaPicker.Default.CapturePhotoAsync();

                if (photo != null)
                {
                    // Store the captured photo path
                    _imagePath = photo.FullPath;

                    // Display the captured image immediately
                    CapturedImage.Source = ImageSource.FromFile(photo.FullPath);
                }
            }
            catch (Exception ex)
            {
                // Handle errors during photo capture
                await DisplayAlert("Error", $"Unable to capture photo: {ex.Message}", "OK");
            }
        }

        private void OnColorSelected(object sender, EventArgs e)
        {
            // Get the selected color from the Picker
            string selectedColor = (string)((Picker)sender).SelectedItem;

            // Define HSV ranges for each color
            switch (selectedColor)
            {
                case "Pink":
                    _lowerBound = new MCvScalar(140, 50, 50);
                    _upperBound = new MCvScalar(170, 255, 255);
                    break;
                case "Yellow":
                    _lowerBound = new MCvScalar(20, 50, 50);
                    _upperBound = new MCvScalar(30, 255, 255);
                    break;
                case "Blue":
                    _lowerBound = new MCvScalar(100, 50, 50);
                    _upperBound = new MCvScalar(130, 255, 255);
                    break;
                case "Green":
                    _lowerBound = new MCvScalar(40, 50, 50);
                    _upperBound = new MCvScalar(80, 255, 255);
                    break;
                case "Purple":
                    _lowerBound = new MCvScalar(125, 50, 50);
                    _upperBound = new MCvScalar(140, 255, 255);
                    break;
                case "Black":
                    _lowerBound = new MCvScalar(0, 0, 0);
                    _upperBound = new MCvScalar(180, 255, 50);
                    break;
                case "Orange":
                    _lowerBound = new MCvScalar(10, 50, 50);
                    _upperBound = new MCvScalar(20, 255, 255);
                    break;
                case "Red":
                    _lowerBound = new MCvScalar(0, 50, 50);
                    _upperBound = new MCvScalar(10, 255, 255);
                    break;
                case "White":
                    _lowerBound = new MCvScalar(0, 0, 200);
                    _upperBound = new MCvScalar(180, 30, 255);
                    break;
                case "Seafoam":
                    _lowerBound = new MCvScalar(85, 50, 50);
                    _upperBound = new MCvScalar(100, 255, 255);
                    break;
                default:
                    return;
            }

            ConfirmButton.IsVisible = true;
        }

        private async void OnConfirmButtonClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_imagePath))
            {
                await DisplayAlert("Error", "Please capture a photo first.", "OK");
                return;
            }

            // Navigate to the AnnotationPage and pass the image path and color bounds
            await Navigation.PushAsync(new AnnotationPage(_imagePath, _lowerBound, _upperBound));
        }
    }
}