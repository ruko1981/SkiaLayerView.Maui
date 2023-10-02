using SkiaSharp.Views.Maui.Handlers;

namespace SkiaLayerView;
public static class Registration
{
   public static MauiAppBuilder UseSKLayerView (this MauiAppBuilder builder)
   {
      builder.ConfigureMauiHandlers(h =>
      {
         //h.AddHandler<SKLayerView, SKCanvasViewHandler>();
         h.AddHandler<SKLayerView2, SKCanvasViewHandler>();
      });

      return builder;
   }
}
