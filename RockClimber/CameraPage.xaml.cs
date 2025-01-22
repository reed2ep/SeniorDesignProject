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
        private string _imagePath;

        public CameraPage()
        {
            InitializeComponent();
        }

        public CameraPage(string imagePath) : this()
        {
            _imagePath = imagePath;

            // Display the image in the UI
            CapturedImage.Source = ImageSource.FromFile(imagePath);

            // Start processing the image
            ProcessImage(new MCvScalar(0, 0, 0), new MCvScalar(255, 255, 255));
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

        private async void ProcessImage(MCvScalar lowerBound, MCvScalar upperBound)
        {
            try
            {
                if (string.IsNullOrEmpty(_imagePath))
                {
                    await DisplayAlert("Error", "Please capture a photo first.", "OK");
                    return;
                }

                // Run processing on a background thread
                await Task.Run(() =>
                {
                    // Load the captured image as a Mat
                    Mat capturedImage = CvInvoke.Imread(_imagePath, Emgu.CV.CvEnum.ImreadModes.Color);

                    // Detect holds for the selected color
                    List<System.Drawing.Rectangle> holds = BlobDetector.DetectHoldsByColor(capturedImage, lowerBound, upperBound);

                    // Update the UI with the processed image
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        DisplayHolds(capturedImage, holds);
                    });
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing image: {ex.Message}");
            }
        }

        private void DisplayHolds(Mat image, List<System.Drawing.Rectangle> holds)
        {
            foreach (var hold in holds)
            {
                // Draw bounding box around each hold
                CvInvoke.Rectangle(image, hold, new MCvScalar(0, 255, 0), 2); // Green rectangle
            }

            // Convert Mat to Android Bitmap and display
            var androidBitmap = BitmapFromMat(image);

            CapturedImage.Source = ImageSource.FromStream(() =>
            {
                var memoryStream = new MemoryStream();
                androidBitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, memoryStream);
                memoryStream.Position = 0;

                androidBitmap.Recycle();
                androidBitmap.Dispose();

                return memoryStream;
            });
        }

        private static Android.Graphics.Bitmap BitmapFromMat(Mat mat)
        {
            using (var image = mat.ToImage<Bgr, byte>()) // Convert Mat to Emgu Image
            {
                byte[] imageBytes = image.ToJpegData(100); // Convert to JPEG data
                return BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length); // Decode as Android Bitmap
            }
        }

        private void OnColorSelected(object sender, EventArgs e)
        {
            // Get the selected color from the Picker
            string selectedColor = (string)((Picker)sender).SelectedItem;

            // Define HSV ranges for each color
            MCvScalar lowerBound, upperBound;

            switch (selectedColor)
            {
                case "Pink":
                    lowerBound = new MCvScalar(140, 50, 50);
                    upperBound = new MCvScalar(170, 255, 255);
                    break;
                case "Yellow":
                    lowerBound = new MCvScalar(20, 50, 50);
                    upperBound = new MCvScalar(30, 255, 255);
                    break;
                case "Blue":
                    lowerBound = new MCvScalar(100, 50, 50);
                    upperBound = new MCvScalar(130, 255, 255);
                    break;
                case "Green":
                    lowerBound = new MCvScalar(40, 50, 50);
                    upperBound = new MCvScalar(80, 255, 255);
                    break;
                case "Purple":
                    lowerBound = new MCvScalar(125, 50, 50);
                    upperBound = new MCvScalar(140, 255, 255);
                    break;
                case "Black":
                    lowerBound = new MCvScalar(0, 0, 0);
                    upperBound = new MCvScalar(180, 255, 50);
                    break;
                case "Orange":
                    lowerBound = new MCvScalar(10, 50, 50);
                    upperBound = new MCvScalar(20, 255, 255);
                    break;
                case "Red":
                    lowerBound = new MCvScalar(0, 50, 50);
                    upperBound = new MCvScalar(10, 255, 255);
                    break;
                case "White":
                    lowerBound = new MCvScalar(0, 0, 200);
                    upperBound = new MCvScalar(180, 30, 255);
                    break;
                case "Seafoam":
                    lowerBound = new MCvScalar(85, 50, 50);
                    upperBound = new MCvScalar(100, 255, 255);
                    break;
                default:
                    return;
            }

            // Process the image with the selected color range
            ProcessImage(lowerBound, upperBound);
        }
    }
}
