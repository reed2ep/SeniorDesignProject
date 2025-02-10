using Emgu.CV.Structure;
using Emgu.CV;

namespace RockClimber
{
    public partial class AnnotationPage : ContentPage
    {
        private readonly string _imagePath;
        private readonly MCvScalar _lowerBound;
        private readonly MCvScalar _upperBound;

        public AnnotationPage(string imagePath, MCvScalar lowerBound, MCvScalar upperBound)
        {
            InitializeComponent();
            _imagePath = imagePath;
            _lowerBound = lowerBound;
            _upperBound = upperBound;

            // Initialize HoldTypePicker
            InitializeHoldTypePicker();

            // Process the image and display it
            ProcessAndDisplayImage();

            // Populate the start and end pickers
            PopulateStartAndEndPickers();
        }

        public enum HoldType
        {
            Jug = 1,
            Crimp = 2,
            Sloper = 3
        }

        private Dictionary<int, (System.Drawing.Rectangle Rect, HoldType Type)> _holds = new();

        private async void ProcessAndDisplayImage()
        {
            try
            {
                var (processedImage, detectedHolds) = await Task.Run(() =>
                {
                    Mat capturedImage = CvInvoke.Imread(_imagePath, Emgu.CV.CvEnum.ImreadModes.Color);
                    var holds = BlobDetector.DetectHoldsByColor(capturedImage, _lowerBound, _upperBound);

                    // Highlight detected holds
                    for (int i = 0; i < holds.Count; i++)
                    {
                        CvInvoke.Rectangle(capturedImage, holds[i], new MCvScalar(0, 255, 0), 2); // Green rectangle
                        CvInvoke.PutText(
                            capturedImage,
                            $"Hold {i + 1}",
                            new System.Drawing.Point(holds[i].X, holds[i].Y - 10),
                            Emgu.CV.CvEnum.FontFace.HersheySimplex,
                            0.6,
                            new MCvScalar(0, 255, 0),
                            2);
                    }

                    return (capturedImage, holds);
                });

                if (detectedHolds == null || detectedHolds.Count == 0)
                {
                    await DisplayAlert("No Holds Detected", "No holds were detected for the selected color.", "OK");
                    return;
                }

                // Initialize holds with default HoldType (Jug)
                _holds = detectedHolds
                    .Select((rect, index) => new { Index = index, Rect = rect })
                    .ToDictionary(h => h.Index, h => (h.Rect, HoldType.Jug));

                DisplayProcessedImage(processedImage);
                PopulateHoldsDropdown();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing image: {ex.Message}");
                await DisplayAlert("Error", "Failed to process the image.", "OK");
            }
        }

        private void PopulateHoldsDropdown()
        {
            var holdItems = _holds.Keys.Select(i => $"Hold {i + 1}").ToList();

            HoldsPicker.ItemsSource = holdItems;
            StartHoldPicker.ItemsSource = holdItems;
            EndHoldPicker.ItemsSource = holdItems;

            // Set default values for Start and End Pickers
            if (holdItems.Any())
            {
                StartHoldPicker.SelectedIndex = 0; // Default to the first hold
                EndHoldPicker.SelectedIndex = holdItems.Count - 1; // Default to the last hold
            }
            HoldsPicker.SelectedIndex = -1;

            HoldsPicker.SelectedIndexChanged += OnHoldSelectionChanged;

            // Add a second StartHoldPicker for the second start hold
            SecondStartHoldPicker.ItemsSource = holdItems;
            SecondStartHoldPicker.SelectedIndex = 1; // Default to the second hold
            SecondStartHoldPicker.SelectedIndexChanged += OnSecondStartHoldChanged;
        }

        private void OnHoldSelectionChanged(object sender, EventArgs e)
        {
            if (HoldsPicker.SelectedIndex >= 0)
            {
                int selectedHoldIndex = HoldsPicker.SelectedIndex;
                var holdType = _holds[selectedHoldIndex].Type;

                // Update HoldTypePicker to reflect the current type
                HoldTypePicker.SelectedIndex = (int)holdType - 1; // Enum values are 1-based
            }
        }

        private void InitializeHoldTypePicker()
        {
            HoldTypePicker.ItemsSource = Enum.GetValues(typeof(HoldType)).Cast<HoldType>().Select(t => t.ToString()).ToList();
            HoldTypePicker.SelectedIndexChanged += OnHoldTypeChanged;
        }

        private void OnHoldTypeChanged(object sender, EventArgs e)
        {
            if (HoldsPicker.SelectedIndex >= 0 && HoldTypePicker.SelectedIndex >= 0)
            {
                int selectedHoldIndex = HoldsPicker.SelectedIndex;
                HoldType selectedType = (HoldType)Enum.Parse(typeof(HoldType), HoldTypePicker.SelectedItem.ToString());

                // Update the hold type in the dictionary
                var hold = _holds[selectedHoldIndex];
                _holds[selectedHoldIndex] = (hold.Rect, selectedType);

                // Redraw the image with updated values
                RedrawProcessedImage();
            }
        }

        private void RedrawProcessedImage()
        {
            // Reload the original image
            Mat originalImage = CvInvoke.Imread(_imagePath, Emgu.CV.CvEnum.ImreadModes.Color);

            // Redraw the rectangles and hold types
            foreach (var kvp in _holds)
            {
                var index = kvp.Key;
                var rect = kvp.Value.Rect;
                var type = kvp.Value.Type;

                CvInvoke.Rectangle(originalImage, rect, new MCvScalar(0, 255, 0), 2); // Green rectangle
                CvInvoke.PutText(
                    originalImage,
                    $"Hold {index + 1} ({type})",
                    new System.Drawing.Point(rect.X, rect.Y - 10), // Place text above the rectangle
                    Emgu.CV.CvEnum.FontFace.HersheySimplex,
                    0.6,
                    new MCvScalar(0, 255, 0),
                    2);
            }

            // Display the updated image
            CapturedImage.Source = ImageSource.FromStream(() =>
            {
                var memoryStream = new MemoryStream();
                using (var androidBitmap = BitmapFromMat(originalImage))
                {
                    androidBitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, memoryStream);
                    memoryStream.Position = 0;
                }
                return memoryStream;
            });
        }

        private void RefreshHoldsDisplay()
        {
            HoldsPicker.ItemsSource = _holds
                .Select(kvp => $"Hold {kvp.Key + 1}: {kvp.Value.Type}")
                .ToList();
        }

        private void DisplayProcessedImage(Mat image)
        {
            foreach (var kvp in _holds)
            {
                var index = kvp.Key;
                var rect = kvp.Value.Rect;
                var type = kvp.Value.Type;

                CvInvoke.Rectangle(image, rect, new MCvScalar(0, 255, 0), 2);
                CvInvoke.PutText(
                    image,
                    $"Hold {index + 1} ({type})",
                    new System.Drawing.Point(rect.X, rect.Y - 10),
                    Emgu.CV.CvEnum.FontFace.HersheySimplex,
                    0.6,
                    new MCvScalar(0, 255, 0),
                    2);
            }

            CapturedImage.Source = ImageSource.FromStream(() =>
            {
                var memoryStream = new MemoryStream();
                using (var androidBitmap = BitmapFromMat(image))
                {
                    androidBitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, memoryStream);
                    memoryStream.Position = 0;
                }
                return memoryStream;
            });
        }

        private void PopulateStartAndEndPickers()
        {
            var holdItems = _holds.Keys.Select(i => $"Hold {i + 1}").ToList();

            StartHoldPicker.ItemsSource = holdItems;
            EndHoldPicker.ItemsSource = holdItems;

            // Set default values: first hold for Start and last hold for End
            if (holdItems.Any())
            {
                StartHoldPicker.SelectedIndex = 0; // Default to the first hold
                EndHoldPicker.SelectedIndex = holdItems.Count - 1; // Default to the last hold
            }

            // Optional: Add event handlers for changes
            StartHoldPicker.SelectedIndexChanged += OnStartHoldChanged;
            EndHoldPicker.SelectedIndexChanged += OnEndHoldChanged;

            // Add a second StartHoldPicker for the second start hold
            SecondStartHoldPicker.ItemsSource = holdItems;
            SecondStartHoldPicker.SelectedIndex = 1; // Default to the second hold
            SecondStartHoldPicker.SelectedIndexChanged += OnSecondStartHoldChanged;
        }

        private void OnStartHoldChanged(object sender, EventArgs e)
        {
            if (StartHoldPicker.SelectedIndex >= 0)
            {
                var selectedStartHold = StartHoldPicker.SelectedItem.ToString();
                Console.WriteLine($"Start Hold changed to: {selectedStartHold}");
            }
        }

        private void OnSecondStartHoldChanged(object sender, EventArgs e)
        {
            if (SecondStartHoldPicker.SelectedIndex >= 0)
            {
                var selectedSecondStartHold = SecondStartHoldPicker.SelectedItem.ToString();
                Console.WriteLine($"Second Start Hold changed to: {selectedSecondStartHold}");
            }
        }

        private void OnEndHoldChanged(object sender, EventArgs e)
        {
            if (EndHoldPicker.SelectedIndex >= 0)
            {
                var selectedEndHold = EndHoldPicker.SelectedItem.ToString();
                Console.WriteLine($"End Hold changed to: {selectedEndHold}");
            }
        }

        private static Android.Graphics.Bitmap BitmapFromMat(Mat mat)
        {
            using (var image = mat.ToImage<Bgr, byte>()) // Convert Mat to Emgu Image
            {
                byte[] imageBytes = image.ToJpegData(100); // Convert to JPEG data
                return Android.Graphics.BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length); // Decode as Android Bitmap
            }
        }

        private async void OnContinueClicked(object sender, EventArgs e)
        {
            if (StartHoldPicker.SelectedIndex >= 0 && EndHoldPicker.SelectedIndex >= 0 && SecondStartHoldPicker.SelectedIndex >= 0)
            {
                // Retrieve selected start and end holds
                int startIndex = StartHoldPicker.SelectedIndex;
                int secondStartIndex = SecondStartHoldPicker.SelectedIndex;
                int endIndex = EndHoldPicker.SelectedIndex;

                var startHold = _holds[startIndex].Rect;
                var secondStartHold = _holds[secondStartIndex].Rect;
                var endHold = _holds[endIndex].Rect;

                double maxReach = 150; // Adjust based on climber ability

                // Find best path between two start holds and the end hold
                var path = ClimbingGraph.FindBestRoute(_holds.Values.Select(h => h.Rect).ToList(), startHold, secondStartHold, endHold, maxReach);

                if (path == null || path.Count == 0)
                {
                    await DisplayAlert("No Path Found", "No valid climbing path was found.", "OK");
                }
                else
                {
                    DisplayPath(path);
                }
            }
            else
            {
                await DisplayAlert("Error", "Please select both start holds and the end hold.", "OK");
            }
        }

        private void DisplayPath(List<Node> path)
        {
            // Reload the original image
            Mat annotatedImage = CvInvoke.Imread(_imagePath, Emgu.CV.CvEnum.ImreadModes.Color);

            // Draw the path
            for (int i = 0; i < path.Count - 1; i++)
            {
                var start = path[i].Hold;
                var end = path[i + 1].Hold;

                System.Drawing.Point startPoint = new System.Drawing.Point(start.X + start.Width / 2, start.Y + start.Height / 2);
                System.Drawing.Point endPoint = new System.Drawing.Point(end.X + end.Width / 2, end.Y + end.Height / 2);

                CvInvoke.Line(annotatedImage, startPoint, endPoint, new MCvScalar(0, 0, 255), 2);
            }

            // Display image
            CapturedImage.Source = ImageSource.FromStream(() =>
            {
                var memoryStream = new MemoryStream();
                using (var androidBitmap = BitmapFromMat(annotatedImage))
                {
                    androidBitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, memoryStream);
                    memoryStream.Position = 0;
                }
                return memoryStream;
            });
        }
    }
}
