using System;
using Heleus.Apps.Shared;
using Heleus.Apps.Shared.Apple.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ExtEditor), typeof(Heleus.Apps.Shared.iOS.Renderers.ExtendedEditorRenderer))]
[assembly: ExportRenderer(typeof(ExtEntry), typeof(Heleus.Apps.Shared.iOS.Renderers.ExtendedEntryRenderer))]

namespace Heleus.Apps.Shared.iOS.Renderers
{
	class ExtendedEditorRenderer : EditorRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
		{
			base.OnElementChanged(e);

			if (Control != null)
			{
				var editor = Element as ExtEditor;
				if (editor == null)
					return;

				editor.ThemeChangedAction = ThemeChanged;
				Control.Font = editor.ToUIFont();
				Control.BackgroundColor = UIColor.Clear;
				ThemeChanged();
			}
		}

		void ThemeChanged()
		{
			Control.TintColor = Theme.TextColor.Color.ToUIColor();
		}
	}

	class ExtendedEntryRenderer : EntryRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			base.OnElementChanged(e);

			if (Control != null)
			{

				var entry = Element as ExtEntry;
				if (entry == null)
					return;

				entry.ThemeChangedAction = ThemeChanged;
				Control.Font = entry.ToUIFont();
				Control.BorderStyle = UITextBorderStyle.None;
				Control.BackgroundColor = UIColor.Clear;
				ThemeChanged();
			}
		}

		void ThemeChanged()
		{
			Control.TintColor = Theme.TextColor.Color.ToUIColor();
		}
	}
}
