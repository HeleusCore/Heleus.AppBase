using System;
using Android.Content;
using Heleus.Apps.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ExtEntry), typeof(Heleus.Apps.Shared.Android.Renderers.ExtendedEntryRenderer))]

namespace Heleus.Apps.Shared.Android.Renderers
{
	public class ExtendedEntryRenderer : EntryRenderer
	{
		public ExtendedEntryRenderer(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			base.OnElementChanged(e);
			if (Control != null)
			{
				ExtendedEditorRenderer.UpdateCursor(Control);

				try
				{
					Control.Background = null;
					Control.SetBackgroundColor(global::Android.Graphics.Color.Transparent);
				}
				catch
				{
				}
			}
		}

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            Enabled = false;
            Enabled = true;
            if (Control != null)
            {
                // https://stackoverflow.com/questions/37566303/edittext-giving-error-textview-does-not-support-text-selection-selection-canc/40140869
                Control.Enabled = false;
                Control.Enabled = true;
            }
        }
    }
}
