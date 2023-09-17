using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace SkiaLayerView;

// All the code in this file is included in all platforms.
public class SKLayerView : SKGLView
{
   private Thread _renderThread = null;
   private AutoResetEvent _threadGate = null;
   private bool _keepSwimming = true;
   private SKPoint _mousePos = new SKPoint();
   private bool _showGrid = true;
   private Point _prevMouseLoc = new Point();

   private Dictionary<string, Layer> _layers;

   public SKLayerView ()
   {
      _layers = new Dictionary<string, Layer>()
      {
         { "background", new Layer("Background Layer") },
         { "grid", new Layer ("Grid Layer") },
         { "data", new Layer ("Data Layer") },
         { "overlay", new Layer ("Overlay Layer") }
      };

      // Create a background rendering thread
      _renderThread = new Thread(RenderLoopMethod);
      _threadGate = new AutoResetEvent(false);

      // Start the rendering thread
      _renderThread.Start();
   }

   protected override void 











   private void RenderLoopMethod (object obj) => throw new NotImplementedException();











   protected override void OnPaintSurface (SKPaintGLSurfaceEventArgs e) => base.OnPaintSurface(e);

   protected override void OnTouch (SKTouchEventArgs e) => base.OnTouch(e);
}
