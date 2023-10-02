using SkiaLayerView;
using SkiaSharp;

namespace SkiaLayerViewSample.Views;

public partial class MainPage : ContentPage
{
   public MainPage (MainViewModel viewModel)
   {
      BindingContext = viewModel;

      Content = new Border()
      {     
         Stroke = Brush.Green,
         Content = new SKLayerView2()
         {

         }
      };
   }
}
