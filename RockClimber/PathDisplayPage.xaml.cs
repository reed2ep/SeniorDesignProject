using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Maui.Controls;
using Android.Graphics;

namespace RockClimber
{
    public partial class PathDisplayPage : ContentPage
    {
        private DatabaseHelper _databaseHelper;
        private List<Move> _routeMoves;
        private int _currentMoveIndex;
        private string _imagePath;

        public PathDisplayPage(List<Move> routeMoves, string imagePath)
        {
            InitializeComponent();

            // Ensure database is initialized
            //string dbPath = Path.Combine(FileSystem.AppDataDirectory, "savedPaths.db3");
            string dbPath = System.IO.Path.Combine(FileSystem.AppDataDirectory, "savedPaths.db3");
            _databaseHelper = new DatabaseHelper(dbPath);

            _routeMoves = routeMoves;
            _imagePath = imagePath;
            _currentMoveIndex = 0;
            DisplaySequentialRoute(_currentMoveIndex);
        }

        private void OnNextMoveClicked(object sender, EventArgs e)
        {
            if (_currentMoveIndex < _routeMoves.Count)
            {
                _currentMoveIndex++;
                DisplaySequentialRoute(_currentMoveIndex);
            }
        }

        private void OnPreviousMoveClicked(object sender, EventArgs e)
        {
            if (_currentMoveIndex > 0)
            {
                _currentMoveIndex--;
                DisplaySequentialRoute(_currentMoveIndex);
            }
        }
        private void OnViewFinalPathClicked(object sender, EventArgs e)
        {
            // Display all moves at once.
            DisplaySequentialRoute(_routeMoves.Count);
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            // Navigate back to the AnnotationPage.
            await Navigation.PopAsync();
        }

        // Helper: Returns an offset for each limb.
        private System.Drawing.Point GetOffsetForLimb(Limb limb)
        {
            switch (limb)
            {
                case Limb.RightHand:
                    return new System.Drawing.Point(0, 0);
                case Limb.LeftHand:
                    return new System.Drawing.Point(3, 3);
                case Limb.RightLeg:
                    return new System.Drawing.Point(-3, -3);
                case Limb.LeftLeg:
                    return new System.Drawing.Point(3, -3);
                default:
                    return new System.Drawing.Point(0, 0);
            }
        }

        // Helper: Returns a color for each limb.
        private MCvScalar GetColorForLimb(Limb limb)
        {
            switch (limb)
            {
                case Limb.RightHand:
                    return new MCvScalar(0, 0, 255);   // Red
                case Limb.LeftHand:
                    return new MCvScalar(255, 0, 0);   // Blue
                case Limb.RightLeg:
                    return new MCvScalar(0, 255, 0);   // Green
                case Limb.LeftLeg:
                    return new MCvScalar(0, 255, 255); // Yellow
                default:
                    return new MCvScalar(0, 0, 0);     // Black 
            }
        }

        // Draws a move with a given offset.
        private void DrawMoveWithOffset(Mat image, Move move, System.Drawing.Point offset)
        {
            var startRect = move.From;
            var endRect = move.To;
            System.Drawing.Point startPoint = new System.Drawing.Point(
                (int)(startRect.X + startRect.Width / 2 + offset.X),
                (int)(startRect.Y + startRect.Height / 2 + offset.Y));
            System.Drawing.Point endPoint = new System.Drawing.Point(
                (int)(endRect.X + endRect.Width / 2 + offset.X),
                (int)(endRect.Y + endRect.Height / 2 + offset.Y));
            MCvScalar color = GetColorForLimb(move.Limb);
            CvInvoke.Line(image, startPoint, endPoint, color, 2);
        }

        // Displays the annotated image with moves up to the specified index.
        private void DisplaySequentialRoute(int upToMoveIndex)
        {
            Mat annotatedImage = CvInvoke.Imread(_imagePath, Emgu.CV.CvEnum.ImreadModes.Color);
            for (int i = 0; i < upToMoveIndex && i < _routeMoves.Count; i++)
            {
                var move = _routeMoves[i];
                System.Drawing.Point offset = GetOffsetForLimb(move.Limb);
                DrawMoveWithOffset(annotatedImage, move, offset);
            }
            PathImage.Source = ImageSource.FromStream(() =>
            {
                MemoryStream memoryStream = new MemoryStream();
                using (var androidBitmap = BitmapFromMat(annotatedImage))
                {
                    androidBitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, memoryStream);
                    memoryStream.Position = 0;
                }
                return memoryStream;
            });
        }

        // Converts an Emgu CV Mat to an Android Bitmap.
        private Android.Graphics.Bitmap BitmapFromMat(Mat mat)
        {
            using (var image = mat.ToImage<Bgr, byte>())
            {
                byte[] imageBytes = image.ToJpegData(100);
                return Android.Graphics.BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
            }
        }
        private async void OnSavePathClicked(object sender, EventArgs e)
        {
            if (_routeMoves.Count == 0)
            {
                await DisplayAlert("Error", "No path to save.", "OK");
                return;
            }

            string defaultName = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // Default name based on date/time

            // Prompt user to enter a name for the path
            string userEnteredName = await DisplayPromptAsync("Save Path", "Enter a name for this path:",
                initialValue: defaultName,
                maxLength: 50,
                keyboard: Keyboard.Text);

            string finalName = string.IsNullOrWhiteSpace(userEnteredName) ? defaultName : userEnteredName;

            // Save to database
            _databaseHelper.SavePath(finalName, _routeMoves, _imagePath);

            await DisplayAlert("Success", "Path saved successfully!", "OK");
        }

    }
}
