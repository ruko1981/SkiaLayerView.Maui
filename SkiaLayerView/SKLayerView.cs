using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace SkiaLayerView;

// All the code in this file is included in all platforms.
public class SKLayerView : SKGLView
{
   private Thread _renderThread = null;
   private AutoResetEvent _threadGate = null;
   private bool _keepSwimming = true;
   private Point _touchLoc = new();
   private bool _showGrid = true;
   private Point _prevTouchLoc = new();

   TapGestureRecognizer TapGesture = new() { Buttons = ButtonsMask.Primary, NumberOfTapsRequired = 1 };

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

      TapGesture.Tapped += (s, e) =>
      {
         // Save the mouse position
         _touchLoc = (Point)e.GetPosition((View)s);

         // Invalidate the Data Layer to draw a new random set of bars
         _layers["data"].Invalidate();

         // If Mouse Move, draw new mouse coordinates
         if (_touchLoc != _prevTouchLoc)
         {
            // Remember the previous mouse location
            _prevTouchLoc = _touchLoc;

            // Invalidate the Overlay Layer to show the new mouse coordinates
            _layers["overlay"].Invalidate();
         }

         // Start a new rendering cycle to redraw any invalidated layers.
         UpdateDrawing();
      };

      GestureRecognizers.Add(TapGesture);

      // Create a background rendering thread
      _renderThread = new Thread(RenderLoopMethod);
      _threadGate = new AutoResetEvent(false);

      // Start the rendering thread
      _renderThread.Start();

   }


   protected override void OnPaintSurface (SKPaintGLSurfaceEventArgs e)
   {
      base.OnPaintSurface(e);

      var canvas = e.Surface.Canvas;

      canvas.Clear(SKColors.Black);

      // Paint each pre-rendered layer onto the Canvas using this GUI thread
      foreach (var layer in _layers.Values)
      {
         layer.Paint(canvas);
      }

      using SKPaint paint = new() { Color = SKColors.LimeGreen };

      for (int i = 0; i < _layers.Count; i++)
      {
         var layer = _layers.Values.ElementAt(i);
         var text = $"{layer.Title} - Renders = {layer.RenderCount}, Paints = {layer.PaintCount}";
         var textLoc = new SKPoint(10, 10 + (i * 15));

         canvas.DrawText(text, textLoc, paint);
      }


      paint.Color = SKColors.Cyan;

      canvas.DrawText("Click-Drag to update bars.", new SKPoint(10, 80), paint);
      canvas.DrawText("Double-Click to show / hide grid.", new SKPoint(10, 95), paint);
      canvas.DrawText("Resize to update all.", new SKPoint(10, 110), paint);

   }

   protected override SizeRequest OnMeasure (double widthConstraint, double heightConstraint)
   {
      foreach (var layer in _layers.Values)
         layer.Invalidate();

      UpdateDrawing();

      return base.OnMeasure(widthConstraint, heightConstraint);
   }

   private void UpdateDrawing ()
   {
      // Unblock the rendering thread to begin a render cycle.  Only the invalidated
      // Layers will be re-rendered, but all will be repainted onto the SKGLControl.
      _threadGate.Set();
   }

   private void RenderLoopMethod (object obj)
   {
      while (_keepSwimming)
      {
         // Draw any invalidated layers using this Render thread
         DrawLayers();

         // Invalidate the SKGLView to run the PaintSurface event on the GUI thread
         // The PaintSurface event will Paint the layer stack to the SKGLView
         this.InvalidateSurface();

         // DoEvents to ensure that the GUI has time to process
         //Application.DoEvents();

         // Block and wait for the next rendering cycle
         _threadGate.WaitOne();
      }
   }

   private void DrawLayers ()
   {
      // Iterate through the collection of layers and raise the Draw event for each layer that is
      // invalidated.  Each event handler will receive a Canvas to draw on along with the Bounds for 
      // the Canvas, and can then draw the contents of that layer. The Draw commands are recorded and  
      // stored in an SKPicture for later playback to the SKGLControl.  This method can be called from
      // any thread.

      //skglControl1.ClientRectangle.ToSKRect();
      _layers["background"].Render(Bounds.ToSKRect(), (canvas, rect) =>
      {
         using SKPaint paint = new() { Color = SKColors.Black };
         canvas.DrawRect(rect, paint);
      });
   }
}
