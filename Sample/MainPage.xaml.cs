using Sample.View;

namespace Sample
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        //private async Task ShowVirtualGrid_Clicked(object sender, EventArgs e)
        //{
        //    await Navigation.PushAsync(new VirtualGridDemoPage());
        //}

        private async void ShowVirtualGrid_Clicked(object sender, EventArgs e)
        {
           await Shell.Current.GoToAsync(nameof(VirtualGridDemoPage));
        }
    }

}
