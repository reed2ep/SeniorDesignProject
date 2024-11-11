using Microsoft.Maui.Controls;

namespace RockClimber
{
    public partial class ImagePage : ContentPage
    {
        public ImagePage(string imagePath)
        {
            InitializeComponent();
            SelectedImage.Source = ImageSource.FromFile(imagePath);
        }

    }
}
