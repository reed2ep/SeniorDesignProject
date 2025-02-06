using Emgu.CV.Structure;
using Emgu.CV;

namespace RockClimber
{
    public partial class AnnotationPage : ContentPage
    {
        private readonly string _imagePath;
        private readonly MCvScalar _lowerBound;
        private readonly MCvScalar _upperBound;
        private Dictionary<int, (System.Drawing.Rectangle Rect, HoldType Type)> _holds = new();

        public AnnotationPage(string imagePath, MCvScalar lowerBound, MCvScalar upperBound)
        {
            InitializeComponent();
            _imagePath = imagePath;
            _lowerBound = lowerBound;
            _upperBound = upperBound;


            InitializeHoldTypePicker();
            ProcessAndDisplayImage();

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
                if (!File.Exists(_imagePath))
                {
                    await DisplayAlert("Error", "Image file not found!", "OK");
                    return;
                }

                var (processedImage, detectedHolds) = await Task.Run(() =>
                {
                    Mat capturedImage = CvInvoke.Imread(_imagePath, Emgu.CV.CvEnum.ImreadModes.Color);
                    var holds = BlobDetector.DetectHoldsByColor(capturedImage, _lowerBound, _upperBound);

                    Console.WriteLine($"Detected Holds Count: {holds?.Count}");

                    for (int i = 0; i < holds.Count; i++)
                    {
                        CvInvoke.Rectangle(capturedImage, holds[i], new MCvScalar(0, 255, 0), 2);
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
                    await DisplayAlert("No Holds Detected", "No holds detected. Try adjusting the detection parameters.", "OK");
                    return;
                }

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

        private async Task DisplayAlert(string v1, string v2, string v3)
        {
            throw new NotImplementedException();
        }

        private void DisplayProcessedImage(Mat processedImage)
        {
            CapturedImage.Source = ImageSource.FromStream(() =>
            {
                var memoryStream = new MemoryStream();
                using (var systemDrawingBitmap = BitmapFromMat(processedImage)) // Fix: use appropriate Bitmap class for your platform
                {
                    systemDrawingBitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    memoryStream.Position = 0;
                }
                return memoryStream;
            });
        }

        private void PopulateHoldsDropdown()
        {
            if (_holds.Count == 0) return;

            var holdItems = _holds.Keys.Select(i => $"Hold {i + 1}").ToList();

            HoldsPicker.ItemsSource = holdItems;
            StartHoldPickerLeft.ItemsSource = holdItems;
            StartHoldPickerRight.ItemsSource = holdItems;
            EndHoldPicker.ItemsSource = holdItems;

            if (holdItems.Count > 1)
            {
                StartHoldPickerLeft.SelectedIndex = 0;
                StartHoldPickerRight.SelectedIndex = 1;
                EndHoldPicker.SelectedIndex = holdItems.Count - 1;
            }

            HoldsPicker.SelectedIndex = -1;
            HoldsPicker.SelectedIndexChanged += OnHoldSelectionChanged;
        }

        private void OnHoldSelectionChanged(object sender, EventArgs e)
        {
            if (HoldsPicker.SelectedIndex >= 0)
            {
                int selectedHoldIndex = HoldsPicker.SelectedIndex;
                var holdType = _holds[selectedHoldIndex].Type;
                HoldTypePicker.SelectedIndex = (int)holdType - 1;
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

                var hold = _holds[selectedHoldIndex];
                _holds[selectedHoldIndex] = (hold.Rect, selectedType);

                RedrawProcessedImage();
            }
        }

        private void RedrawProcessedImage()
        {
            Mat originalImage = CvInvoke.Imread(_imagePath, Emgu.CV.CvEnum.ImreadModes.Color);

            foreach (var kvp in _holds)
            {
                var index = kvp.Key;
                var rect = kvp.Value.Rect;
                var type = kvp.Value.Type;

                CvInvoke.Rectangle(originalImage, rect, new MCvScalar(0, 255, 0), 2);
                CvInvoke.PutText(
                    originalImage,
                    $"Hold {index + 1} ({type})",
                    new System.Drawing.Point(rect.X, rect.Y - 10),
                    Emgu.CV.CvEnum.FontFace.HersheySimplex,
                    0.6,
                    new MCvScalar(0, 255, 0),
                    2);
            }

            DisplayProcessedImage(originalImage);
        }

        private static System.Drawing.Bitmap BitmapFromMat(Mat mat)
        {
            using (var image = mat.ToImage<Bgr, byte>())
            {
                byte[] imageBytes = image.ToJpegData(100);
                using (var memoryStream = new MemoryStream(imageBytes))
                {
                    return new System.Drawing.Bitmap(memoryStream);
                }
            }
        }

        private async void OnContinueClicked(object sender, EventArgs e)
        {
            if (StartHoldPickerLeft.SelectedIndex >= 0 && StartHoldPickerRight.SelectedIndex >= 0 && EndHoldPicker.SelectedIndex >= 0)
            {
                var leftStartHold = StartHoldPickerLeft.SelectedItem.ToString();
                var rightStartHold = StartHoldPickerRight.SelectedItem.ToString();
                var endHold = EndHoldPicker.SelectedItem.ToString();

                await DisplayAlert("Continue", $"Start Holds: Left - {leftStartHold}, Right - {rightStartHold}\nEnd Hold: {endHold}", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Please select both start holds and an end hold.", "OK");
            }
        }
    }
}
