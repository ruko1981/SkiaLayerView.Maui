namespace SkiaLayerViewSample.Resources.Styles;

public static class AppStyles
{
	public static object Get(string resourceName)
	{
		if (App.Current.Resources.ContainsKey(resourceName))
		{
			return App.Current.Resources[resourceName];
		}
		else
		{
			foreach (var mergeDict in App.Current.Resources.MergedDictionaries)
			{
				if (mergeDict.Keys.Contains(resourceName))
				{
					return mergeDict[resourceName];
				}
			}
		}

		return null;
	}
}
