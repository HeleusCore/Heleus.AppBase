using System;
using Android.Content;
using Android.Views;
using Android.Widget;
using Heleus.Apps.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ExtEditor), typeof(Heleus.Apps.Shared.Android.Renderers.ExtendedEditorRenderer))]

namespace Heleus.Apps.Shared.Android.Renderers
{
	public class ExtendedEditorRenderer : EditorRenderer
	{
		public ExtendedEditorRenderer(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
		{
			base.OnElementChanged(e);

			if (Control != null)
			{
				UpdateCursor(Control);

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

        public static void UpdateCursor(EditText editText)
		{
			try
			{
				// https://github.com/android/platform_frameworks_base/blob/kitkat-release/core/java/android/widget/TextView.java#L562-564
				var f = Java.Lang.Class.FromType(typeof(global::Android.Widget.TextView)).GetDeclaredField("mCursorDrawableRes");
				f.Accessible = true;
				//f.Set(editText, Droid.Resource.Drawable.cursor);

			}
			catch
			{
			}
		}
	}
}
