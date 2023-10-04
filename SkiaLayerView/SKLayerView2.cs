using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace SkiaLayerView;
public class SKLayerView2 : SKCanvasView
{
   public static readonly BindableProperty AmplitudesProperty = BindableProperty.Create(nameof(Amplitudes), typeof(List<SKLayer>), typeof(SKLayerView2), default(List<SKLayer>), propertyChanged: OnDataChanged);
   public static readonly BindableProperty MarkersProperty = BindableProperty.Create(nameof(Markers), typeof(List<TimeSpan>), typeof(SKLayerView2), default(List<TimeSpan>), propertyChanged: OnMarkersChanged);
   public static readonly BindableProperty ShowGridProperty = BindableProperty.Create(nameof(ShowGrid), typeof(bool), typeof(SKLayerView2), true, propertyChanged: OnShowGridChanged);
   public static readonly BindableProperty ShowOverlayProperty = BindableProperty.Create(nameof(ShowOverlay), typeof(bool), typeof(SKLayerView2), false, propertyChanged: OnShowOverlayChanged);

   
   public List<SKLayer> Amplitudes
   {
      get => (List<SKLayer>)GetValue(AmplitudesProperty);
      set => SetValue(AmplitudesProperty, value);
   }
   public List<TimeSpan> Markers
   {
      get => (List<TimeSpan>)GetValue(MarkersProperty);
      set => SetValue(MarkersProperty, value);
   }
   public bool ShowGrid
   {
      get => (bool)GetValue(ShowGridProperty);
      set => SetValue(ShowGridProperty, value);
   }
   public bool ShowOverlay
   {
      get => (bool)GetValue(ShowOverlayProperty);
      set => SetValue(ShowOverlayProperty, value);
   }

   public Dictionary<string, SKLayer> Layers = new();
   private List<string> _layerOrder = new();

   private SemaphoreSlim _renderSemaphore = new SemaphoreSlim(1, 1);
   private CancellationTokenSource _cts;

   Action<SKCanvas, SKRect> BackgroundDrawAction = (canvas, rect) =>
   {
      using SKPaint paint = new() { Color = SKColors.Red };
      SKRect r = new(0, rect.Top, 10, rect.Bottom);

      canvas.DrawRect(r, paint);
   };
   Action<SKCanvas, SKRect> GridDrawAction = (canvas, rect) =>
   {
      using SKPaint paint = new() { Color = SKColors.Yellow };
      SKRect r = new(10, rect.Top, 20, rect.Bottom);

      canvas.DrawRect(r, paint);
   };
   Action<SKCanvas, SKRect> DataDrawAction = (canvas, rect) =>
   {
      using SKPaint paint = new() { Color = SKColors.Green };
      SKRect r = new(20, rect.Top, 30, rect.Bottom);

      canvas.DrawRect(r, paint);
   };
   Action<SKCanvas, SKRect> OverlayDrawAction = (canvas, rect) =>
   {
      using SKPaint paint = new() { Color = SKColors.Blue };
      SKRect r = new(30, rect.Top, 40, rect.Bottom);

      canvas.DrawRect(r, paint);
   };
   Action<SKCanvas, SKRect> MarkersDrawAction = (canvas, rect) =>
   {
      using SKPaint paint = new() { Color = SKColors.Violet };
      SKRect r = new(40, rect.Top, 50, rect.Bottom);

      canvas.DrawRect(r, paint);
   };

   public SKLayerView2 ()
   {
      Layers.Add("background", new SKLayer("Background Layer", BackgroundDrawAction));
      Layers.Add("grid", new SKLayer("Grid Layer", GridDrawAction));
      Layers.Add("data", new SKLayer("Amplitudes Layer", DataDrawAction));
      Layers.Add("markers", new SKLayer("Markers Layer", MarkersDrawAction));

      Layers.Add("overlay", new SKLayer("Overlay Layer", OverlayDrawAction));

      _layerOrder = new List<string>()
      {
         "background",
         "grid",
         "data",
         "markers",
         "overlay"
      };

      StartRenderLoop();
      InvalidateSurface();
   }

   private async Task RenderLoop ()
   {
      var token = _cts.Token;

      while (!token.IsCancellationRequested)
      {
         try
         {
            await _renderSemaphore.WaitAsync(token);

            var bounds = Bounds.ToSKRect();

            // TODO: Add ShouldRender flag
            foreach (var layerName in _layerOrder)
            {
               Layers[layerName].Render(bounds);
            }

            // Invalidate the SKCanvasView to trigger a PaintSurface event
            InvalidateSurface();

            // Simulate a delay, adjust as needed
            await Task.Delay(16, token); // 60 FPS
         }

         catch (Exception)//(OperationCanceledException)
         {
            // Handle cancellation
         }
         finally
         {
            _renderSemaphore.Release();
         }
      }
   }
   public void StartRenderLoop ()
   {
      _cts?.Cancel();
      _cts?.Dispose();

      _cts = new CancellationTokenSource();
      
      Task.Run(() => RenderLoop());
   }
   public void StopRenderLoop ()
   {
      _cts?.Cancel();
      _cts?.Dispose();
   }

   override protected void OnPaintSurface (SKPaintSurfaceEventArgs e)
   {
      base.OnPaintSurface(e);

      var canvas = e.Surface.Canvas;

      // Draw the layers to the SKCanvas
      foreach (var layerName in _layerOrder)
      {
         Layers[layerName].Paint(canvas);
      }
   }

   

   private static void OnShowGridChanged (BindableObject bindable, object oldValue, object newValue) => throw new NotImplementedException();
   private static void OnMarkersChanged (BindableObject bindable, object oldValue, object newValue) => throw new NotImplementedException();
   private static void OnDataChanged (BindableObject bindable, object oldValue, object newValue) => throw new NotImplementedException();
   private static void OnShowOverlayChanged (BindableObject bindable, object oldValue, object newValue) => throw new NotImplementedException();
}



/*
#region CRUD Operations
public void AddLayer (string name, Action<SKCanvas, SKRect> drawAction)
{
   if (ContainsLayer(name))
      throw new ArgumentException($"Layer with name {name} already exists.");
   Layers.Add(name, new SKLayer(name, drawAction));
   _layerOrder.Add(name);
}
public void AddLayer (string name, Action<SKCanvas, SKRect> drawAction, int index)
{
   if (ContainsLayer(name))
      throw new ArgumentException($"Layer with name {name} already exists.");

   _layerOrder.Insert(index, name);
   Layers.Add(name, new SKLayer(name, drawAction));
}
public void AddLayer (string name, Action<SKCanvas, SKRect> drawAction, string targetLayerName, bool insertAbove = true)
{
   if (ContainsLayer(name))
      throw new ArgumentException($"Layer with name {name} already exists.");
   if (!ContainsLayer(targetLayerName))
      throw new ArgumentException($"Target layer with name {targetLayerName} does not exist.");


   int index = _layerOrder.IndexOf(targetLayerName) + (insertAbove ? 1 : 0);
   AddLayer(name, drawAction, index);

}
public void AddLayers (IEnumerable<(string Name, Action<SKCanvas, SKRect> DrawAction)> layers)
{
   foreach (var (name, drawAction) in layers)
   {
      AddLayer(name, drawAction);
   }
}
public void AddLayers (IEnumerable<(string Name, Action<SKCanvas, SKRect> DrawAction)> layers, int index)
{
   foreach (var (name, drawAction) in layers)
   {
      AddLayer(name, drawAction, index);
   }
}
public void AddLayers (IEnumerable<(string Name, Action<SKCanvas, SKRect> DrawAction)> layers, string targetLayerName, bool insertAbove = true)
{
   foreach (var (name, drawAction) in layers)
   {
      AddLayer(name, drawAction, targetLayerName, insertAbove);
   }
}
public void ClearLayers ()
{
   _layerOrder.Clear();
   Layers.Clear();
}
public SKLayer GetLayer (string name)
{
   if (!ContainsLayer(name))
      throw new ArgumentException($"Layer with name {name} does not exist.");
   return Layers[name];
}
public int GetLayerCount () => Layers.Count;
public int GetLayerIndex (string name)
{
   if (!ContainsLayer(name))
      throw new ArgumentException($"Layer with name {name} does not exist.");

   return _layerOrder.IndexOf(name);
}
public void RemoveLayer (string name)
{
   if (!ContainsLayer(name))
      throw new ArgumentException($"Layer with name {name} does not exist.");

   _layerOrder.Remove(name);
   Layers.Remove(name);
}
public void RemoveLayer (SKLayer layer) => RemoveLayer(layer.Name);
public void RemoveLayers (IEnumerable<string> names)
{
   foreach (var name in names)
   {
      RemoveLayer(name);
   }
}
public void RenameLayer (string oldName, string newName)
{
   if (!ContainsLayer(oldName))
      throw new ArgumentException($"Layer with name {oldName} does not exist.");
   if (ContainsLayer(newName))
      throw new ArgumentException($"Layer with name {newName} already exists.");

   _layerOrder[_layerOrder.IndexOf(oldName)] = newName;

   var layer = Layers[oldName];
   Layers.Remove(oldName);
   Layers.Add(newName, layer);
}
public bool TryAddLayer (string name, Action<SKCanvas, SKRect> drawAction)
{
   if (ContainsLayer(name))
      return false;
   try
   {
      AddLayer(name, drawAction);
      return true;
   }
   catch
   {
      return false;
   }
}
public bool TryAddLayer (string name, Action<SKCanvas, SKRect> drawAction, int index)
{
   if (ContainsLayer(name))
      return false;
   try
   {
      AddLayer(name, drawAction, index);
      return true;
   }
   catch
   {
      return false;
   }
}
public bool TryAddLayer (string name, Action<SKCanvas, SKRect> drawAction, string targetLayerName, bool insertAbove = true)
{
   if (ContainsLayer(name))
      return false;
   try
   {
      AddLayer(name, drawAction, targetLayerName, insertAbove);
      return true;
   }
   catch
   {
      return false;
   }
}
public bool TryAddLayers (IEnumerable<(string Name, Action<SKCanvas, SKRect> DrawAction)> layers)
{
   bool shouldAdd = false;
   foreach (var (name, drawAction) in layers)
   {
      shouldAdd |= ContainsLayer(name);
   }
   try
   {
      if (shouldAdd)
         AddLayers(layers);
      return shouldAdd;
   }
   catch
   {
      return false;
   }
}
public bool TryAddLayers (IEnumerable<(string Name, Action<SKCanvas, SKRect> DrawAction)> layers, int index)
{
   bool shouldAdd = false;
   foreach (var (name, drawAction) in layers)
   {
      shouldAdd |= ContainsLayer(name);
   }
   try
   {
      if (shouldAdd)
         AddLayers(layers, index);
      return shouldAdd;
   }
   catch
   {
      return false;
   }
}
public bool TryAddLayers (IEnumerable<(string Name, Action<SKCanvas, SKRect> DrawAction)> layers, string targetLayerName, bool insertAbove = true)
{
   bool shouldAdd = false;
   foreach (var (name, drawAction) in layers)
   {
      shouldAdd |= ContainsLayer(name);
   }
   try
   {
      if (shouldAdd)
         AddLayers(layers, targetLayerName, insertAbove);
      return shouldAdd;
   }
   catch
   {
      return false;
   }
}
public bool TryRemoveLayer (string name)
{
   if (!ContainsLayer(name))
      return false;
   try
   {
      RemoveLayer(name);
      return true;
   }
   catch
   {
      return false;
   }
}
public bool TryRemoveLayer (SKLayer layer)
{
   return TryRemoveLayer(layer.Name);
}
public void UpdateLayer (string name, Action<SKCanvas, SKRect> newDrawAction)
{
   if (!ContainsLayer(name))
      throw new ArgumentException($"Layer with name {name} does not exist.");
   Layers[name].DrawAction = newDrawAction;  // Assuming SKLayer has a property named DrawAction
}
#endregion

#region Layer Ordering Operations
public void MoveLayerToTop (string name)
{
   if (!ContainsLayer(name))
      throw new ArgumentException($"Layer with name {name} does not exist.");

   _layerOrder.Remove(name);
   _layerOrder.Add(name);
}
public void MoveLayersToTop (IEnumerable<string> names)
{
   foreach (var name in names)
   {
      MoveLayerToTop(name);
   }
}
public void MoveLayerToBottom (string name)
{
   if (!ContainsLayer(name))
      throw new ArgumentException($"Layer with name {name} does not exist.");

   _layerOrder.Remove(name);
   _layerOrder.Insert(0, name);
}
public void MoveLayersToBottom (IEnumerable<string> names)
{
   foreach (var name in names.Reverse())
   {
      MoveLayerToBottom(name);
   }
}
public void ShiftLayer (string name, bool moveUp = true, int count = 1)
{
   if (!ContainsLayer(name))
      throw new ArgumentException($"Layer with name {name} does not exist.");

   var currentIndex = _layerOrder.IndexOf(name);

   // Calculate the new index based on the direction and count
   var newIndex = moveUp ? currentIndex - count : currentIndex + count;

   // Ensure the new index is within bounds
   if (newIndex < 0 || newIndex >= _layerOrder.Count)
      throw new InvalidOperationException($"Cannot move the layer {name} by {count} positions in the specified direction.");

   // Remove the layer from the current position and insert at the new position
   _layerOrder.RemoveAt(currentIndex);
   _layerOrder.Insert(newIndex, name);
}
public void ShiftLayers (IEnumerable<string> names, bool moveUp = true, int count = 1)
{
   foreach (var name in names)
   {
      ShiftLayer(name, moveUp, count);
   }
}

public void SwapLayers (string layerName1, string layerName2)
{
   if (!ContainsLayer(layerName1))
      throw new ArgumentException($"Layer with name {layerName1} does not exist.");
   if (!ContainsLayer(layerName2))
      throw new ArgumentException($"Layer with name {layerName2} does not exist.");

   int index1 = _layerOrder.IndexOf(layerName1);
   int index2 = _layerOrder.IndexOf(layerName2);

   _layerOrder[index1] = layerName2;
   _layerOrder[index2] = layerName1;
}
#endregion
#region Layer Visibility Operations
public void SetLayerOpacity (string name, float opacity)
{
   if (!ContainsLayer(name))
      throw new ArgumentException($"Layer with name {name} does not exist.");
   Layers[name].Opacity = opacity;  // Assuming SKLayer has a property named Opacity
}
public void SetLayersOpacity (IEnumerable<string> names, float opacity)
{
   foreach (var name in names)
   {
      if (ContainsLayer(name))
      {
         Layers[name].Opacity = opacity; // Assuming SKLayer has a property named Opacity
      }
   }
}
public void SetLayerVisibility (string name, bool isVisible)
{
   if (!ContainsLayer(name))
      throw new ArgumentException($"Layer with name {name} does not exist.");
   Layers[name].IsVisible = isVisible;  // Assuming SKLayer has a property named IsVisible
}
public void SetLayersVisibility (IEnumerable<string> names, bool isVisible)
{
   foreach (var name in names)
   {
      if (ContainsLayer(name))
      {
         Layers[name].IsVisible = isVisible; // Assuming SKLayer has a property named IsVisible
      }
   }
}
public void ToggleAllLayerVisibility ()
{
   foreach (var layer in Layers.Values)
   {
      layer.IsVisible = !layer.IsVisible; // Assuming SKLayer has a property named IsVisible
   }
}
public void ToggleLayerVisibility (string name)
{
   if (!ContainsLayer(name))
      throw new ArgumentException($"Layer with name {name} does not exist.");
   Layers[name].IsVisible = !Layers[name].IsVisible;  // Assuming SKLayer has a property named IsVisible
}
public void ToggleLayersVisibility (IEnumerable<string> names)
{
   foreach (var name in names)
   {
      if (ContainsLayer(name))
      {
         Layers[name].IsVisible = !Layers[name].IsVisible; // Assuming SKLayer has a property named IsVisible
      }
   }
}
#endregion

#region Helper Methods
private bool ContainsLayer (string name) => Layers.ContainsKey(name);
#endregion
*/

//#region IEnumerable<SKLayer> Implementation
//public IEnumerator<SKLayer> GetEnumerator ()
//{
//   foreach (var layerName in _layerOrder)
//   {
//      yield return Layers[layerName];
//   }
//}

//IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();
//#endregion

//public bool AutoRender { get; set; } = true;

//private async Task RenderLoop () { throw new NotImplementedException(); }
//public async Task RenderInvalidLayers () => throw new NotImplementedException();
//public async Task InvalidateLayer (string layerName) => throw new NotImplementedException();


//LayerManager LayerManager = new();
//Layermanager.AddLayer("Background", (c,s) => {} )