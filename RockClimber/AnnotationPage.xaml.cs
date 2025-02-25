using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Maui.Storage;

namespace RockClimber
{
    public partial class AnnotationPage : ContentPage
    {
        private readonly string _imagePath;
        private readonly MCvScalar _lowerBound;
        private readonly MCvScalar _upperBound;
        private Dictionary<int, (System.Drawing.Rectangle Rect, HoldType Type)> _holds = new();

        // End and start hold possible selections
        private int rightHandStartIndex = 0;
        private int leftHandStartIndex = 0;
        private int rightLegStartIndex = 0;
        private int leftLegStartIndex = 0;

        private int rightHandEndIndex = -1;
        private int leftHandEndIndex = -1;


        public AnnotationPage(string imagePath, MCvScalar lowerBound, MCvScalar upperBound)
        {
            InitializeComponent();
            _imagePath = imagePath;
            _lowerBound = lowerBound;
            _upperBound = upperBound;

            // Initialize the height of the wall
            LoadWallHeight();
            // Initialize the HoldType picker 
            InitializeHoldTypePicker();

            // Process the image and display it
            ProcessAndDisplayImage();

            AttachPickerEventHandlers();
        }

        public enum HoldType
        {
            Jug = 1,
            Crimp = 2,
            Sloper = 3
        }

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
                PopulateEndAndLimbPickers();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing image: {ex.Message}");
                await DisplayAlert("Error", "Failed to process the image.", "OK");
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
        private void PopulateHoldsDropdown()
        {
            var holdItems = _holds.Keys.Select(i => $"Hold {i + 1}").ToList();
            HoldsPicker.ItemsSource = holdItems;
            HoldsPicker.SelectedIndex = -1;
            HoldsPicker.SelectedIndexChanged += OnHoldSelectionChanged;
        }
        private void PopulateEndAndLimbPickers()
        {
            var holdItems = _holds.Keys.Select(i => $"Hold {i + 1}").ToList();

            // Populate one hand end hold picker
            EndHoldPicker.ItemsSource = holdItems;

            // Populate R/L end hold pickers
            RightHandEndPicker.ItemsSource = holdItems;
            LeftHandEndPicker.ItemsSource = holdItems;

            // Populate one hand start hold picker
            StartHoldPicker.ItemsSource = holdItems;

            // Populate R/L start hold pickers
            RightStartPicker.ItemsSource = holdItems;
            LeftStartPicker.ItemsSource = holdItems;

            // Populate one leg start hold picker
            StartLegPicker.ItemsSource = holdItems;

            // Populate R/L leg start hold pickers
            RightLegPicker.ItemsSource = holdItems;
            LeftLegPicker.ItemsSource = holdItems;
        }

        private void OnHoldSelectionChanged(object sender, EventArgs e)
        {
            if (HoldsPicker.SelectedIndex >= 0)
            {
                int selectedHoldIndex = HoldsPicker.SelectedIndex;
                HoldTypePicker.SelectedIndex = (int)_holds[selectedHoldIndex].Type - 1;
            }
        }

        private void InitializeHoldTypePicker()
        {
            HoldTypePicker.ItemsSource = Enum.GetValues(typeof(HoldType))
                                             .Cast<HoldType>()
                                             .Select(t => t.ToString())
                                             .ToList();
            HoldTypePicker.SelectedIndexChanged += OnHoldTypeChanged;
            HoldTypePicker.IsEnabled = false;
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
        private void OnEditHoldClicked(object sender, EventArgs e)
        {
            HoldEditSection.IsVisible = !HoldEditSection.IsVisible;
            HoldsPicker.IsEnabled = HoldEditSection.IsVisible;
            HoldTypePicker.IsEnabled = HoldEditSection.IsVisible;
        }

        private void OnEditStartHoldsClicked(object sender, EventArgs e)
        {
            bool isVisible = !StartHoldDisplay.IsVisible;

            // Toggle visibility for start hold selection sections
            StartHoldDisplay.IsVisible = isVisible;
            OneStartCheckSection.IsVisible = isVisible;
            TwoStartCheckSection.IsVisible = isVisible;
            OneLegCheckSection.IsVisible = isVisible;
            TwoLegCheckSection.IsVisible = isVisible;

            // Toggle visibility for end hold selection sections
            EndHoldDisplay.IsVisible = isVisible;
            OneEndCheckSection.IsVisible = isVisible;
            TwoEndCheckSection.IsVisible = isVisible;
        }

        private void OnOneHandEndChecked(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                TwoHandEndCheckBox.IsChecked = false; // Uncheck the other checkbox
            }

            EndHoldSection.IsVisible = e.Value;
            EndHoldPicker.IsEnabled = e.Value;
        }

        private void OnTwoHandEndChecked(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                OneHandEndCheckBox.IsChecked = false; // Uncheck the other checkbox
            }

            TwoEndHoldSection.IsVisible = e.Value;
            RightHandEndPicker.IsEnabled = e.Value;
            LeftHandEndPicker.IsEnabled = e.Value;
        }

        private void OnOneHandStartChecked(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                TwoHandStartCheckBox.IsChecked = false; // Uncheck the other checkbox
            }

            StartHoldSection.IsVisible = e.Value;
            StartHoldPicker.IsEnabled = e.Value;
        }

        private void OnTwoHandStartChecked(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                OneHandStartCheckBox.IsChecked = false; // Uncheck the other checkbox
            }

            TwoStartHoldSection.IsVisible = e.Value;
            RightStartPicker.IsEnabled = e.Value;
            LeftStartPicker.IsEnabled = e.Value;
        }

        private void OnOneLegChecked(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                TwoLegStartCheckBox.IsChecked = false; // Uncheck the other checkbox
            }

            OneLegStartHoldSection.IsVisible = e.Value;
            StartLegPicker.IsEnabled = e.Value;
        }

        private void OnTwoLegChecked(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                OneLegStartCheckBox.IsChecked = false; // Uncheck the other checkbox
            }

            TwoLegStartHoldSection.IsVisible = e.Value;
            RightLegPicker.IsEnabled = e.Value;
            LeftLegPicker.IsEnabled = e.Value;
        }

        private void SaveSelections(object sender, EventArgs e)
        {
            // Save Start Hold Selections
            if (StartHoldPicker.SelectedIndex >= 0)
            {
                rightHandStartIndex = StartHoldPicker.SelectedIndex;
                leftHandStartIndex = StartHoldPicker.SelectedIndex;
                Console.WriteLine($"Start Hold selected: {StartHoldPicker.SelectedItem}");
            }

            if (RightStartPicker.SelectedIndex >= 0)
            {
                rightHandStartIndex = RightStartPicker.SelectedIndex;
                Console.WriteLine($"Right Hand Start Hold: {RightStartPicker.SelectedItem}");
            }

            if (LeftStartPicker.SelectedIndex >= 0)
            {
                leftHandStartIndex = LeftStartPicker.SelectedIndex;
                Console.WriteLine($"Left Hand Start Hold: {LeftStartPicker.SelectedItem}");
            }

            if (StartLegPicker.SelectedIndex >= 0)
            {
                rightLegStartIndex = StartLegPicker.SelectedIndex;
                leftLegStartIndex = StartLegPicker.SelectedIndex;
                Console.WriteLine($"Leg Start Hold: {StartLegPicker.SelectedItem}");
            }

            if (RightLegPicker.SelectedIndex >= 0)
            {
                rightLegStartIndex = RightLegPicker.SelectedIndex;
                Console.WriteLine($"Right Leg Start Hold: {RightLegPicker.SelectedItem}");
            }

            if (LeftLegPicker.SelectedIndex >= 0)
            {
                leftLegStartIndex = LeftLegPicker.SelectedIndex;
                Console.WriteLine($"Left Leg Start Hold: {LeftLegPicker.SelectedItem}");
            }

            // Save End Hold Selections
            if (EndHoldPicker.SelectedIndex >= 0)
            {
                rightHandEndIndex = EndHoldPicker.SelectedIndex;
                leftHandEndIndex = EndHoldPicker.SelectedIndex;
                Console.WriteLine($"End Hold selected: {EndHoldPicker.SelectedItem}");
            }

            if (RightHandEndPicker.SelectedIndex >= 0)
            {
                rightHandEndIndex = RightHandEndPicker.SelectedIndex;
                Console.WriteLine($"Right Hand End Hold: {RightHandEndPicker.SelectedItem}");
            }

            if (LeftHandEndPicker.SelectedIndex >= 0)
            {
                leftHandEndIndex = LeftHandEndPicker.SelectedIndex;
                Console.WriteLine($"Left Hand End Hold: {LeftHandEndPicker.SelectedItem}");
            }
        }


        private void AttachPickerEventHandlers()
        {
            EndHoldPicker.SelectedIndexChanged += SaveSelections;
            RightHandEndPicker.SelectedIndexChanged += SaveSelections;
            LeftHandEndPicker.SelectedIndexChanged += SaveSelections;
            StartHoldPicker.SelectedIndexChanged += SaveSelections;
            RightStartPicker.SelectedIndexChanged += SaveSelections;
            LeftStartPicker.SelectedIndexChanged += SaveSelections;
            RightLegPicker.SelectedIndexChanged += SaveSelections;
            LeftLegPicker.SelectedIndexChanged += SaveSelections;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Success", "Hold details updated successfully!", "OK");

            // Hide editing sections
            HoldEditSection.IsVisible = false;
            EndHoldSection.IsVisible = false;
            TwoEndHoldSection.IsVisible = false;
            StartHoldSection.IsVisible = false;
            TwoStartHoldSection.IsVisible = false;
            OneLegStartHoldSection.IsVisible = false;
            TwoLegStartHoldSection.IsVisible = false;

            // Hide checkbox sections
            EndHoldDisplay.IsVisible = false;
            OneEndCheckSection.IsVisible = false;
            TwoEndCheckSection.IsVisible = false;
            StartHoldDisplay.IsVisible = false;
            OneStartCheckSection.IsVisible = false;
            TwoStartCheckSection.IsVisible = false;
            OneLegCheckSection.IsVisible = false;
            TwoLegCheckSection.IsVisible = false;

            // Disable pickers
            HoldsPicker.IsEnabled = false;
            HoldTypePicker.IsEnabled = false;
            EndHoldPicker.IsEnabled = false;
            RightHandEndPicker.IsEnabled = false;
            LeftHandEndPicker.IsEnabled = false;
            StartHoldPicker.IsEnabled = false;
            RightStartPicker.IsEnabled = false;
            LeftStartPicker.IsEnabled = false;
            StartLegPicker.IsEnabled = false;
            LeftLegPicker.IsEnabled = false;
            RightLegPicker.IsEnabled = false;
        }

        private async void OnContinueClicked(object sender, EventArgs e)
        {
            // Log the selected indices.
            Console.WriteLine($"Right Hand Start Hold: {rightHandStartIndex}");
            Console.WriteLine($"Left Hand Start Hold: {leftHandStartIndex}");
            Console.WriteLine($"Right Leg Start Hold: {rightLegStartIndex}");
            Console.WriteLine($"Left Leg Start Hold: {leftLegStartIndex}");
            Console.WriteLine($"Right Hand Finish Hold: {rightHandEndIndex}");
            Console.WriteLine($"Left Hand Finish Hold: {leftHandEndIndex}");

            // Check that at least one hand start hold and one finish hold are selected.
            if ((rightHandStartIndex == -1 && leftHandStartIndex == -1) ||
                (rightHandEndIndex == -1 && leftHandEndIndex == -1))
            {
                await DisplayAlert("Error", "Please select at least one hand start hold and one finish hold.", "OK");
                return;
            }

            // Retrieve user wingspan and compute max reach.
            int wingspanFeet = Preferences.Get("wingspanFeet", 5);
            int wingspanInches = Preferences.Get("wingspanInches", 0);
            double wingspanTotal = wingspanFeet + (wingspanInches / 12.0);
            // Effective arm length is roughly half the wingspan, adjusted by a factor (e.g., 0.9) to account for shoulder position.
            double effectiveArmLengthFeet = (wingspanTotal / 2.0) * 0.9;
            double maxReachPixels = ConvertFeetToPixels(effectiveArmLengthFeet);

            // Retrieve all detected holds.
            var allHolds = _holds.Values.Select(h => h.Rect).ToList();

            // Retrieve start holds for hands and legs.
            var rightHandStartHold = _holds[rightHandStartIndex].Rect;
            var leftHandStartHold = _holds[leftHandStartIndex].Rect;
            var rightLegStartHold = _holds[rightLegStartIndex].Rect;
            var leftLegStartHold = _holds[leftLegStartIndex].Rect;

            // Retrieve finish holds for hands.
            var rightHandFinishHold = _holds[rightHandEndIndex].Rect;
            // If a left-hand finish hold isn’t provided, use the right-hand finish for both.
            System.Drawing.Rectangle? leftHandFinishHold = _holds.ContainsKey(leftHandEndIndex)
                ? _holds[leftHandEndIndex].Rect
                : (System.Drawing.Rectangle?)null;

            // Build the initial configuration.
            var startConfig = new LimbConfiguration
            {
                RightHand = rightHandStartHold,
                LeftHand = leftHandStartHold,
                RightLeg = rightLegStartHold,
                LeftLeg = leftLegStartHold
            };

            List<Move> routeMoves = null;
            try
            {
                routeMoves = RoutePlanner.PlanSequentialRoute(
                    allHolds,
                    startConfig,
                    rightHandFinishHold,
                    leftHandFinishHold,
                    maxReachPixels);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
                return;
            }

            if (routeMoves == null || routeMoves.Count == 0)
            {
                await DisplayAlert("No Path Found", "No valid sequential climbing moves were found.", "OK");
                return;
            }

            // Optionally, log the moves.
            foreach (var move in routeMoves)
            {
                Console.WriteLine($"{move.Limb} moved from {move.From} to {move.To}");
            }

            // Now, draw the sequence of moves.
            DisplaySequentialRoute(routeMoves);
        }

        private void DisplaySequentialRoute(List<Move> moves)
        {
            // Reload the original image.
            Mat annotatedImage = CvInvoke.Imread(_imagePath, Emgu.CV.CvEnum.ImreadModes.Color);

            // For each move, draw a small line from the "from" center to the "to" center.
            foreach (var move in moves)
            {
                var color = new MCvScalar(0, 0, 0); // default black
                                                    // Choose a color based on the limb.
                switch (move.Limb)
                {
                    case Limb.RightHand:
                        color = new MCvScalar(0, 0, 255); // Red
                        break;
                    case Limb.LeftHand:
                        color = new MCvScalar(255, 0, 0); // Blue
                        break;
                    case Limb.RightLeg:
                        color = new MCvScalar(0, 255, 0); // Green
                        break;
                    case Limb.LeftLeg:
                        color = new MCvScalar(0, 255, 255); // Yellow
                        break;
                }
                var startRect = move.From;
                var endRect = move.To;
                System.Drawing.Point startPoint = new System.Drawing.Point(startRect.X + startRect.Width / 2, startRect.Y + startRect.Height / 2);
                System.Drawing.Point endPoint = new System.Drawing.Point(endRect.X + endRect.Width / 2, endRect.Y + endRect.Height / 2);
                CvInvoke.Line(annotatedImage, startPoint, endPoint, color, 2);
            }

            // Update the displayed image.
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


        private void DrawPathWithOffset(Mat image, List<Node> path, MCvScalar color, int offsetX, int offsetY)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                var startRect = path[i].Hold;
                var endRect = path[i + 1].Hold;
                System.Drawing.Point startPoint = new System.Drawing.Point(
                    startRect.X + startRect.Width / 2 + offsetX,
                    startRect.Y + startRect.Height / 2 + offsetY);
                System.Drawing.Point endPoint = new System.Drawing.Point(
                    endRect.X + endRect.Width / 2 + offsetX,
                    endRect.Y + endRect.Height / 2 + offsetY);
                CvInvoke.Line(image, startPoint, endPoint, color, 2);
            }
        }


        // Helper method to draw a path by connecting the centers of holds.
        private void DrawPath(Mat image, List<Node> path, MCvScalar color)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                var startRect = path[i].Hold;
                var endRect = path[i + 1].Hold;
                System.Drawing.Point startPoint = new System.Drawing.Point(startRect.X + startRect.Width / 2, startRect.Y + startRect.Height / 2);
                System.Drawing.Point endPoint = new System.Drawing.Point(endRect.X + endRect.Width / 2, endRect.Y + endRect.Height / 2);
                CvInvoke.Line(image, startPoint, endPoint, color, 2);
            }
        }

        // Helper to convert an Emgu CV Mat to an Android Bitmap.
        private static Android.Graphics.Bitmap BitmapFromMat(Mat mat)
        {
            using (var image = mat.ToImage<Bgr, byte>())
            {
                byte[] imageBytes = image.ToJpegData(100);
                return Android.Graphics.BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
            }
        }

        private void LoadWallHeight()
        {
            // Retrieve stored wall height or default to 15 feet
            double wallHeightFeet = Preferences.Get("wallHeightFeet", 15.0);
            WallHeightEntry.Text = wallHeightFeet.ToString();
        }

        private void OnChangeWallHeightClicked(object sender, EventArgs e)
        {
            if (!WallHeightSection.IsVisible)
            {
                WallHeightSection.IsVisible = true;
            }
            else
            {
                if (double.TryParse(WallHeightEntry.Text, out double newWallHeight) && newWallHeight > 0)
                {
                    Preferences.Set("wallHeightFeet", newWallHeight);
                    DisplayAlert("Success", $"Wall height set to {newWallHeight} feet.", "OK");
                    WallHeightSection.IsVisible = false; // Hide the input field after saving
                }
                else
                {
                    DisplayAlert("Error", "Please enter a valid wall height.", "OK");
                }
            }
        }

        private double ConvertFeetToPixels(double feet)
        {
            // Retrieve stored wall height (default: 15 feet)
            double wallHeightFeet = Preferences.Get("wallHeightFeet", 15.0);

            // Load the climbing wall image and get height in pixels
            Mat capturedImage = CvInvoke.Imread(_imagePath, Emgu.CV.CvEnum.ImreadModes.Color);
            int wallHeightPixels = capturedImage.Rows; // Get the image height in pixels

            // Compute scaling factor (pixels per foot)
            double pixelsPerFoot = wallHeightPixels / wallHeightFeet;

            // Convert feet to pixels
            return feet * pixelsPerFoot;
        }
    }
}