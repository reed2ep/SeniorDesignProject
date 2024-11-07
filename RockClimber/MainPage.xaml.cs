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
            await Navigation.PushAsync(new CameraPage());
        }

    }

}
