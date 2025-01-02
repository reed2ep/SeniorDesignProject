using CommunityToolkit.Maui.Media;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using Android.Graphics; // For Android.Graphics.Bitmap
using System.Drawing; // For System.Drawing types

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
                // Capture photo using MediaPicker
                var photo = await MediaPicker.Default.CapturePhotoAsync();

                if (photo != null)
                {
                    // Perform blob detection
                    OnCameraCaptureCompleted(photo.FullPath);
                }
            }
            catch (Exception ex)
            {
                // Handle errors during photo capture
                await DisplayAlert("Error", $"Unable to capture photo: {ex.Message}", "OK");
            }
        }

        private async void OnCameraCaptureCompleted(string imagePath)
        {
            try
            {
                // Display the captured image in the UI before processing
                CapturedImage.Source = ImageSource.FromFile(imagePath);

                // Load the captured image as a Mat
                Mat capturedImage = CvInvoke.Imread(imagePath, Emgu.CV.CvEnum.ImreadModes.Color);

                // Detect blobs
                List<CircleF> detectedBlobs = BlobDetector.DetectBlobs(capturedImage);

                // Display blobs on the image
                DisplayBlobs(capturedImage, detectedBlobs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing image: {ex.Message}");
            }
        }


        private void DisplayBlobs(Mat image, List<CircleF> blobs)
        {
            try
            {
                // Draw all contours for debugging
                Mat debugContours = new Mat();
                CvInvoke.CvtColor(image, debugContours, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray); // Convert to grayscale

                foreach (var blob in blobs)
                {
                    // Draw blobs as green circles
                    CvInvoke.Circle(image, System.Drawing.Point.Round(blob.Center), (int)blob.Radius, new Emgu.CV.Structure.MCvScalar(0, 255, 0), 2);
                }

                // Convert Mat to Android Bitmap
                var androidBitmap = BitmapFromMat(image);

                // Use the Bitmap for the ImageSource
                CapturedImage.Source = ImageSource.FromStream(() =>
                {
                    var memoryStream = new MemoryStream();
                    androidBitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, memoryStream); // Save to stream
                    memoryStream.Position = 0;

                    // Dispose of the bitmap properly
                    androidBitmap.Recycle();
                    androidBitmap.Dispose();

                    return memoryStream;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error displaying blobs: {ex.Message}");
            }
        }



        private static Android.Graphics.Bitmap BitmapFromMat(Mat mat)
        {
            // Convert Mat to a Bitmap
            using (var image = mat.ToImage<Bgr, byte>()) // Convert Mat to Emgu Image
            {
                byte[] imageBytes = image.ToJpegData(100); // Convert to JPEG data
                return BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length); // Decode as Android Bitmap
            }
        }
    }
}