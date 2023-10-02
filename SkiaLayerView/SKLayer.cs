namespace SkiaLayerView;
public class SKLayer
{
   // The finished recording - Used to play back the Draw commands to the SKGLView from the GUI thread
   private SKPicture _picture = null;

   // A flag that indicates if the Layer is valid, or needs to be redrawn.
   private bool _isValid = false;


   public string Name { get; set; }
   public int RenderCount { get; private set; }
   public int PaintCount { get; private set; }
   public bool IsVisible { get; internal set; }
   public float Opacity { get; internal set; }
   public Action<SKCanvas, SKRect> DrawAction { get; internal set; }

   public SKLayer (string name, Action<SKCanvas, SKRect> drawAction)
   {
      Name = name;
      DrawAction = drawAction;
      RenderCount = 0;
      PaintCount = 0;
   }

   public void Render (SKRect clippingBounds)
   {
      // Only redraw the Layer if it has been invalidated
      if (!_isValid)
      {
         // Create an SKPictureRecorder to record the Canvas Draw commands to an SKPicture
         using var recorder = new SKPictureRecorder();

         // Start recording
         recorder.BeginRecording(clippingBounds);

         // Use drawAction action to draw on the Canvas provided and the commands will be recorded for later playback.
         DrawAction(recorder.RecordingCanvas, clippingBounds);

         // Dispose of previous picure
         _picture?.Dispose();

         // Create a new picture from the recording
         _picture = recorder.EndRecording();

         RenderCount++;

         _isValid = true;
      }
   }

   // Paints the previously recorded SKPicture to the provided skglControlCanvas.  This basically plays 
   // back the draw commands from the last Render.  This should be called from the SKGLView.PaintSurface
   // event using the GUI thread.
   public void Paint (SKCanvas SKGLViewCanvas)
   {
      if (_picture is not null)
      {
         SKGLViewCanvas.DrawPicture(_picture);

         PaintCount++;
      }
   }

   // Invalidates the Layer so that it will be redrawn on the next Render call.
   public void Invalidate () => _isValid = false;

}
