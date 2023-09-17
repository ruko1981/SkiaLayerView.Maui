using SkiaLayerView;

namespace SkiaLayerViewSample.Views;

public partial class MainPage : ContentPage
{
	public MainPage(MainViewModel viewModel)
	{
		BindingContext = viewModel;

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Spacing = 25,

				Children =
				{
					new SKLayerView 
					{ 
				 	
					}
				}
			}
			.Padding(30, 0)
			.CenterVertical()
		};
	}
}
