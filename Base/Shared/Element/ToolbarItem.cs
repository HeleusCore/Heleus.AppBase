using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
	public class ExtToolbarItem : ToolbarItem
	{
		public ExtToolbarItem(string name, string icon, Func<Task> activated, ToolbarItemOrder order = ToolbarItemOrder.Default, int priority = 0) :
			base(name, null, () => UIApp.Run(activated), order, priority)
		{
			if (!string.IsNullOrEmpty(icon))
			{
				if (UIApp.IsMacOS)
				{
					icon = icon.Replace(".png", ".pdf");
					IconImageSource = icon;
				}
			}
		}

		public string Identifier = null;
	}
}
