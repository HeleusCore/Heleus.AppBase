using System;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
	public class ExtScrollView : ScrollView, IThemeable
    {
        public Action ThemeChangedAction = null;
		public bool DisableTouchCanel = false;


		public void ThemeChanged()
		{
			Content?.PropagateThemeChange();
            ThemeChangedAction?.Invoke();
        }
    }
}
