using System;
using Heleus.Apps.Shared;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using PRESERVE = Xamarin.Forms.Internals.PreserveAttribute;

[assembly: ExportRenderer(typeof(ExtViewCell), typeof(Heleus.Apps.Shared.iOS.Renderers.ExtendedViewCellRenderer))]

namespace Heleus.Apps.Shared.iOS.Renderers
{
	[PRESERVE]
	class ExtendedViewCellRenderer : ViewCellRenderer
	{
		[PRESERVE]
		public ExtendedViewCellRenderer()
		{
		}

		[PRESERVE]
		public override UIKit.UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
		{
			var cell = base.GetCell(item, reusableCell, tv);
			if (cell != null)
			{
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			}

			return cell;
		}
	}
}
