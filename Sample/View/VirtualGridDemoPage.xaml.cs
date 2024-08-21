using Sample.ViewModel;

namespace Sample.View;

public partial class VirtualGridDemoPage : ContentPage
{
	public VirtualGridDemoPage(VirtualGridViewModel virtualGridViewModel)
	{
		InitializeComponent();
		BindingContext = virtualGridViewModel;

    }

    private void Button_Clicked_Add(object sender, EventArgs e)
    {

    }

    private void Button_Clicked_Remove(object sender, EventArgs e)
    {

    }

    private void Button_Clicked_Replace(object sender, EventArgs e)
    {

    }

    private void Button_Clicked_Move(object sender, EventArgs e)
    {

    }
}