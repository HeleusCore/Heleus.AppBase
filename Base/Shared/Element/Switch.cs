using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
	public class ExtSwitch : Switch, IThemeable
	{
        public bool IgnoreNextToggle;

		public Action ThemeChangedAction = null;

        public new Action<ExtSwitch> Toggled;
        public Func<ExtSwitch, Task> ToggledAsync;

        public ExtSwitch()
        {
            base.Toggled += ExtSwitch_Toggled;
        }

        public void RevertToggle()
        {
            IgnoreNextToggle = true;
            IsToggled = !IsToggled;
        }

        public void SetToogle(bool toggled)
        {
            if (IsToggled != toggled)
            {
                IgnoreNextToggle = true;
                IsToggled = toggled;
            }
        }

        void ExtSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            if (IgnoreNextToggle)
            {
                IgnoreNextToggle = false;
                return;
            }

            Toggled?.Invoke(this);
            if (ToggledAsync != null)
                UIApp.Run(() => ToggledAsync.Invoke(this));
        }

        public void ThemeChanged()
		{
			ThemeChangedAction?.Invoke();
		}
	}
}
