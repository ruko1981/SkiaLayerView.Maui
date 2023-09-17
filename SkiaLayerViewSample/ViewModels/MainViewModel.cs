using System.Linq;

namespace SkiaLayerViewSample.ViewModels;

public partial class MainViewModel : BaseViewModel
{
	int count = 0;

	[ObservableProperty]
	public string message = "Click me";

	[RelayCommand]
	private void OnCounterClicked()
	{
		count++;

		if (count == 1)
			Message = $"Clicked {count} time";
		else
			Message = $"Clicked {count} times";

		SemanticScreenReader.Announce(Message);
	}

	public void DoSomething1()
	{
		for (int i = 0; i < 10; i++)
		{
			System.Diagnostics.Debug.WriteLine(i);
		}
	}

	public void DoSomething2()
	{
		foreach (var i in Enumerable.Range(1, 10))
		{
			System.Diagnostics.Debug.WriteLine(i);
		}
	}
}
