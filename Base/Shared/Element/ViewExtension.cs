using System;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
	public static class ViewExtension
	{
		public static void PropagateThemeChange (this View view)
		{
			if (view is IThemeable) {
				(view as IThemeable).ThemeChanged ();
			}

			if (view is ContentView) {
				PropagateThemeChange ((view as ContentView).Content);
			} else if (view is Layout<View>) {
				var layout = view as Layout<View>;
				foreach (var child in layout.Children)
					PropagateThemeChange (child);
			}
		}

		public static readonly BindableProperty ThemeUseFontWeightProperty = BindableProperty.Create("ThemeUseFontWeight", typeof(bool), typeof(View), true);
		public static readonly BindableProperty ThemeUseFontSizeProperty = BindableProperty.Create("ThemeUseFontSize", typeof(bool), typeof(View), true);

		public static void ThemeUseFontWeight(this View view, bool enabled)
		{
			view.SetValue(ThemeUseFontWeightProperty, enabled);
		}

		public static bool ThemeUseFontWeight(this View view)
		{
			return (bool)view.GetValue(ThemeUseFontWeightProperty);
		}

		public static void ThemeUseFontSize(this View view, bool enabled)
		{
			view.SetValue(ThemeUseFontSizeProperty, enabled);
		}

		public static bool ThemeUseFontSize(this View view)
		{
			return (bool)view.GetValue(ThemeUseFontSizeProperty);
		}
	}
}
