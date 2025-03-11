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

                    // Draw circles with only numbers (No rectangles)
                    for (int i = 0; i < holds.Count; i++)
                    {
                        var rect = holds[i];

                        // Calculate center of the hold
                        var center = new System.Drawing.Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);

                        // Draw a circle
                        CvInvoke.Circle(capturedImage, center, 15, new MCvScalar(0, 255, 0), 2);

                        // Draw the number inside the circle (Only the number)
                        CvInvoke.PutText(
                            capturedImage,
                            $"{i + 1}", // Just the number
                            new System.Drawing.Point(center.X - 10, center.Y + 5),
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

            // Draw circles and label them with updated Hold Types
            foreach (var kvp in _holds)
            {
                var index = kvp.Key;
                var rect = kvp.Value.Rect;
                var holdType = kvp.Value.Type;

                // Determine color based on hold type
                MCvScalar color = holdType switch
                {
                    HoldType.Jug => new MCvScalar(0, 255, 0), // Green
                    HoldType.Crimp => new MCvScalar(255, 0, 0), // Blue
                    HoldType.Sloper => new MCvScalar(255, 255, 0), // Yellow
                    _ => new MCvScalar(0, 255, 0)
                };

                // Calculate the center of the hold
                var center = new System.Drawing.Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);

                // Draw a circle
                CvInvoke.Circle(originalImage, center, 15, color, 2);

                // Draw the number and hold type abbreviation
                string label = $"{index + 1} ({holdType.ToString()[0]})"; // Example: "4 (J)"
                CvInvoke.PutText(originalImage, label, new System.Drawing.Point(center.X - 10, center.Y + 5),
                    Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.6, color, 2);
            }

            // Display the updated image
            CapturedImage.Source = ImageSource.FromStream(() =>
            {
                var memoryStream = new MemoryStream();
                using (var androidBitmap = BitmapFromMat(originalImage))
                {
                    if (androidBitmap != null)
                    {
                        androidBitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, memoryStream);
                        memoryStream.Position = 0;
                    }
                }
                return memoryStream;
            });
        }

        private void DisplayProcessedImage(Mat image)
        {
            // Draw circles with numbers only (no rectangles)
            foreach (var kvp in _holds)
            {
                var index = kvp.Key;
                var rect = kvp.Value.Rect;

                // Calculate the center of the hold
                var center = new System.Drawing.Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);

                // Draw a circle (No rectangle)
                CvInvoke.Circle(image, center, 15, new MCvScalar(0, 255, 0), 2); // Green circle

                // Draw the number inside the circle (No "Hold")
                CvInvoke.PutText(
                    image,
                    $"{index + 1}", // Just the number
                    new System.Drawing.Point(center.X - 10, center.Y + 5), // Center the text
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

        private void InitializeHoldTypePicker()
        {
            HoldTypePicker.ItemsSource = Enum.GetValues(typeof(HoldType))
                                             .Cast<HoldType>()
                                             .Select(t => t.ToString())
                                             .ToList();
            HoldTypePicker.SelectedIndexChanged += OnHoldTypeChanged;
            HoldTypePicker.IsEnabled = false;
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

        #region Event Handlers

        private void OnHoldItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                // Extract selected item as a string
                string selectedHoldString = e.SelectedItem.ToString();

                // Parse hold index from the selected item (assuming format: "1: Jug")
                int selectedHoldIndex = int.Parse(selectedHoldString.Split(':')[0]) - 1;

                // Display a picker or another selection method for the user to choose a new hold type
                DisplayHoldTypePicker(selectedHoldIndex);

                // Clear selection to allow re-selecting the same item
                HoldListView.SelectedItem = null;
            }
        }

        // This function would allow the user to select a new hold type
        private async void DisplayHoldTypePicker(int selectedHoldIndex)
        {
            string[] holdTypeNames = Enum.GetNames(typeof(HoldType));

            string selectedTypeString = await Application.Current.MainPage.DisplayActionSheet(
                "Select Hold Type",
                "Cancel",
                null,
                holdTypeNames
            );

            if (selectedTypeString != null && selectedTypeString != "Cancel")
            {
                HoldType selectedType = (HoldType)Enum.Parse(typeof(HoldType), selectedTypeString);

                // Update the hold type in the dictionary
                var hold = _holds[selectedHoldIndex];
                _holds[selectedHoldIndex] = (hold.Rect, selectedType);

                // Refresh the UI to reflect changes
                HoldListView.ItemsSource = _holds
                    .OrderBy(h => h.Key)
                    .Select(h => $"{h.Key + 1}: {h.Value.Type}")
                    .ToList();

                // Redraw the image with updated values
                RedrawProcessedImage();
            }
        }


        private void OnListHoldTypesClicked(object sender, EventArgs e)
        {
            // Populate the hold list in the correct format: "1: Jug"
            HoldListView.ItemsSource = _holds
                .OrderBy(h => h.Key)
                .Select(h => $"{h.Key + 1}: {h.Value.Type}")
                .ToList();

            // Toggle visibility of the hold list
            HoldListSection.IsVisible = !HoldListSection.IsVisible;

            // Adjust the layout dynamically to show/hide the hold list
            if (HoldListSection.IsVisible)
            {
                // Shrink image section and show the list
                ImageColumn.Width = new GridLength(3, GridUnitType.Star);
                ListColumn.Width = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                // Hide the list and return to full-screen mode
                ImageColumn.Width = new GridLength(1, GridUnitType.Star);
                ListColumn.Width = new GridLength(0, GridUnitType.Absolute);
            }
        }


        private void OnHoldSelectionChanged(object sender, EventArgs e)
        {
            if (HoldsPicker.SelectedIndex >= 0)
            {
                int selectedHoldIndex = HoldsPicker.SelectedIndex;
                HoldTypePicker.SelectedIndex = (int)_holds[selectedHoldIndex].Type - 1;
            }
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

            if (!isVisible) // When hiding, trigger save logic
            {
                OnSaveClicked(sender, e);
            }
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
            Console.WriteLine($"Right Hand Start Hold: {rightHandStartIndex}");
            Console.WriteLine($"Left Hand Start Hold: {leftHandStartIndex}");
            Console.WriteLine($"Right Leg Start Hold: {rightLegStartIndex}");
            Console.WriteLine($"Left Leg Start Hold: {leftLegStartIndex}");
            Console.WriteLine($"Right Hand Finish Hold: {rightHandEndIndex}");
            Console.WriteLine($"Left Hand Finish Hold: {leftHandEndIndex}");

            if ((rightHandStartIndex == -1 && leftHandStartIndex == -1) ||
                (rightHandEndIndex == -1 && leftHandEndIndex == -1))
            {
                await DisplayAlert("Error", "Please select at least one hand start hold and one finish hold.", "OK");
                return;
            }

            int wingspanFeet = Preferences.Get("wingspanFeet", 5);
            int wingspanInches = Preferences.Get("wingspanInches", 0);
            double wingspanTotal = wingspanFeet + (wingspanInches / 12.0);
            // Effective arm length: roughly half the wingspan adjusted by 0.9.
            double effectiveArmLengthFeet = (wingspanTotal / 2.0) * 0.9;
            double maxReachPixels = ConvertFeetToPixels(effectiveArmLengthFeet);

            int climberHeightFeet = Preferences.Get("climberHeightFeet", 5);
            double climberHeightPixels = ConvertFeetToPixels(climberHeightFeet);
            double targetGap = 0.75 * climberHeightPixels;

            var allHolds = _holds.Values.Select(h => h.Rect).ToList();
            var rightHandStartHold = _holds[rightHandStartIndex].Rect;
            var leftHandStartHold = _holds[leftHandStartIndex].Rect;
            var rightLegStartHold = _holds[rightLegStartIndex].Rect;
            var leftLegStartHold = _holds[leftLegStartIndex].Rect;

            var rightHandFinishHold = _holds[rightHandEndIndex].Rect;
            System.Drawing.Rectangle? leftHandFinishHold = _holds.ContainsKey(leftHandEndIndex) ? _holds[leftHandEndIndex].Rect : (System.Drawing.Rectangle?)null;

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
                routeMoves = RoutePlanner.PlanSequentialRoute(allHolds, startConfig, rightHandFinishHold, leftHandFinishHold, maxReachPixels, targetGap);
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

            await Navigation.PushAsync(new PathDisplayPage(routeMoves, _imagePath));

        }
        #endregion

        #region Wall Height and Conversion

        private void LoadWallHeight()
        {
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
                    WallHeightSection.IsVisible = false;
                }
                else
                {
                    DisplayAlert("Error", "Please enter a valid wall height.", "OK");
                }
            }
        }

        private double ConvertFeetToPixels(double feet)
        {
            double wallHeightFeet = Preferences.Get("wallHeightFeet", 15.0);
            Mat capturedImage = CvInvoke.Imread(_imagePath, Emgu.CV.CvEnum.ImreadModes.Color);
            int wallHeightPixels = capturedImage.Rows;
            double pixelsPerFoot = wallHeightPixels / wallHeightFeet;
            return feet * pixelsPerFoot;
        }

        private Android.Graphics.Bitmap BitmapFromMat(Mat mat)
        {
            using (var image = mat.ToImage<Bgr, byte>())
            {
                byte[] imageBytes = image.ToJpegData(100);
                return Android.Graphics.BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
            }
        }
        #endregion

    }
}