using Microsoft.Maui.Storage;
namespace RockClimber
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

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
                var photo = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions
                {
                    Title = "Take a photo"
                });

                if (photo != null)
                {
                    var photoPath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
                    using (var stream = await photo.OpenReadAsync())
                    using (var fileStream = File.OpenWrite(photoPath))
                    {
                        await stream.CopyToAsync(fileStream);
                    }

                    await Navigation.PushAsync(new ImagePage(photoPath));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Unable to take photo: {ex.Message}", "OK");
            }
        }


        private async void OnOpenGalleryClicked(object sender, EventArgs e)
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select an image",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null)
            {
                await Navigation.PushAsync(new ImagePage(result.FullPath));
            }
        }



    }

}
