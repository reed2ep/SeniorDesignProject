using Microsoft.Maui.Storage;
namespace RockClimber
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnGoToProfileClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProfilePage());
        }

        private async void OnGoToCameraPageClicked(object sender, EventArgs e)
        {
            try
            {
                var photo = await MediaPicker.Default.CapturePhotoAsync();

                if (photo != null)
                {
                    // Navigate to CameraPage with the captured photo path
                    await Navigation.PushAsync(new CameraPage(photo.FullPath));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Unable to capture photo: {ex.Message}", "OK");
            }
        }

        private async void OnOpenGalleryClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select an image",
                    FileTypes = FilePickerFileType.Images
                });

                if (result != null)
                {
                    // Navigate to CameraPage with the selected image path
                    await Navigation.PushAsync(new CameraPage(result.FullPath));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Unable to select image: {ex.Message}", "OK");
            }
        }

        private async void OnGoToSavedPathsPageClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SavedPathsPage());
        }

    }

}
