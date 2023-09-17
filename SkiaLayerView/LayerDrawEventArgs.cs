using SkiaSharp;

namespace SkiaLayerView;

public class LayerDrawEventArgs : EventArgs
   {
      public SKRect Bounds { get; set; }
      public SKCanvas Canvas { get; set; }

      public LayerDrawEventArgs (SKCanvas canvas, SKRect bounds)
      {
         Bounds = bounds;
         Canvas = canvas;
      }
   }
