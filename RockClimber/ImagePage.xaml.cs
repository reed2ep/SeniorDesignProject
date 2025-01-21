using Microsoft.Maui.Controls;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Collections.Generic;
using System;
using System.IO;
using Android.Graphics; // For Android.Graphics.Bitmap
using Microsoft.Maui.Dispatching;

namespace RockClimber
{
    public partial class ImagePage : ContentPage
    {
        public ImagePage(string imagePath)
        {
            InitializeComponent();

            // Display the selected image immediately
            SelectedImage.Source = ImageSource.FromFile(imagePath);

            // Start processing the image
            ProcessImage(imagePath);
        }

        private async void ProcessImage(string imagePath)
        {
            // Show the loading indicator
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            try
            {
                // Run image processing on a background thread
                await Task.Run(() =>
                {
                    // Load the image into a Mat
                    Mat selectedImage = CvInvoke.Imread(imagePath, Emgu.CV.CvEnum.ImreadModes.Color);

                    // Resize the image for faster processing
                    Mat resizedImage = new Mat();
                    CvInvoke.Resize(selectedImage, resizedImage, new System.Drawing.Size(300, 300));

                    // Detect blobs
                    //List<CircleF> detectedBlobs = BlobDetector.DetectBlobs(resizedImage);

                    // Update the UI with the processed image
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        //DisplayBlobs(resizedImage, detectedBlobs);
                    });
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing image: {ex.Message}");
            }
            finally
            {
                // Hide the loading indicator
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }

        private void DisplayBlobs(Mat image, List<CircleF> blobs)
        {
            try
            {
                // Draw detected blobs on the image
                foreach (var blob in blobs)
                {
                    CvInvoke.Circle(image, System.Drawing.Point.Round(blob.Center), (int)blob.Radius, new Emgu.CV.Structure.MCvScalar(0, 255, 0), 2);
                }

                // Convert the Mat to an Android Bitmap
                var androidBitmap = BitmapFromMat(image);

                // Display the processed image in the UI
                SelectedImage.Source = ImageSource.FromStream(() =>
                {
                    var memoryStream = new MemoryStream();
                    androidBitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, memoryStream);
                    memoryStream.Position = 0;

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
            using (var image = mat.ToImage<Bgr, byte>()) // Convert Mat to Emgu Image
            {
                byte[] imageBytes = image.ToJpegData(100); // Convert to JPEG data
                return BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length); // Decode as Android Bitmap
            }
        }
    }
}

