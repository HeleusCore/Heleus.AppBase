using System;
using System.ComponentModel;
using AppKit;
using Foundation;
using Heleus.Apps.Shared;
using Heleus.Apps.Shared.Apple.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(ExtEditor), typeof(Heleus.Apps.Shared.macOS.Renderers.ExtEditorRenderer))]

namespace Heleus.Apps.Shared.macOS.Renderers
{
	class FormsNSScrollView : NSScrollView
	{
		public override void ScrollWheel(NSEvent theEvent)
		{
			if (VerticalScroller.Enabled)
			{
				if (theEvent.ScrollingDeltaY > 0) // up
				{
					if (VerticalScroller.FloatValue <= 0)
					{
						NextResponder.ScrollWheel(theEvent);
					}
				}
				else if (theEvent.ScrollingDeltaY < 0) // down
				{
					if (VerticalScroller.FloatValue >= 1)
					{
						NextResponder.ScrollWheel(theEvent);
					}
				}

				base.ScrollWheel(theEvent);
			}
			else
			{
				NextResponder.ScrollWheel(theEvent);
			}
		}
	}

	public class ExtEditorRenderer : ViewRenderer<ExtEditor, NSView>
	{
		const string NewLineSelector = "insertNewline";
		bool _disposed;

		IEditorController ElementController => Element;

		NSTextView textView;

		protected override void OnElementChanged(ElementChangedEventArgs<ExtEditor> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
			{
				textView = new NSTextView();
				textView.RichText = false;
				textView.MaxSize = new CoreGraphics.CGSize(nfloat.MaxValue, nfloat.MaxValue);
				textView.VerticallyResizable = true;
				textView.HorizontallyResizable = false;
				textView.AutoresizingMask = NSViewResizingMask.WidthSizable;

				var scroller = new NSScrollView
				{
					AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable,
					DocumentView = textView,
					HasVerticalScroller = true,
					DrawsBackground = false
				};

				SetNativeControl(scroller);

				textView.TextDidBeginEditing += TextView_TextDidBeginEditing;
				textView.TextDidEndEditing += TextView_TextDidEndEditing;
				textView.DoCommandBySelector += TextView_DoCommandBySelector;
				textView.TextDidChange += TextView_TextDidChange;
			}

			if (e.NewElement == null) return;
			UpdateText();
			UpdateFont();
			UpdateTextColor();
			UpdateEditable();
            UpdateMaxLength();
            UpdateIsReadOnly();
        }

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Editor.TextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateEditable();
			else if (e.PropertyName == Editor.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == Editor.FontAttributesProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Editor.FontFamilyProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Editor.FontSizeProperty.PropertyName)
				UpdateFont();
            else if (e.PropertyName == InputView.MaxLengthProperty.PropertyName)
                UpdateMaxLength();
            else if (e.PropertyName == InputView.IsReadOnlyProperty.PropertyName)
                UpdateIsReadOnly();
        }

		protected override void SetBackgroundColor(Color color)
		{
			if (textView == null)
				return;

			textView.BackgroundColor = color == Color.Default ? NSColor.Clear : color.ToNSColor();

			base.SetBackgroundColor(color);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				if (textView != null)
				{
					textView.TextDidBeginEditing -= TextView_TextDidBeginEditing;
					textView.TextDidEndEditing -= TextView_TextDidEndEditing;
					textView.DoCommandBySelector -= TextView_DoCommandBySelector;

					textView = null;
				}
			}
			base.Dispose(disposing);
		}

		void TextView_TextDidChange(object sender, EventArgs e)
		{
			ElementController.SetValueFromRenderer(Editor.TextProperty, textView.String);
		}

		void TextView_TextDidEndEditing(object sender, EventArgs e)
		{
			Element.SetValue(VisualElement.IsFocusedPropertyKey, false);
			ElementController.SendCompleted();
		}

		void TextView_TextDidBeginEditing(object sender, EventArgs e)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
		}

		bool TextView_DoCommandBySelector(NSTextView textView, ObjCRuntime.Selector commandSelector)
		{
			var result = false;
			if (commandSelector.Name.StartsWith(NewLineSelector, StringComparison.InvariantCultureIgnoreCase))
			{
				textView.InsertText(new NSString(Environment.NewLine));
				result = true;
			}
			return result;
		}

		void UpdateEditable()
		{
			textView.Editable = Element.IsEnabled;
		}

		void UpdateFont()
		{
			textView.Font = Element.ToNSFont();
		}

		void UpdateText()
		{
			if (textView.Value != Element.Text)
				textView.Value = Element.Text ?? string.Empty;
		}

		void UpdateTextColor()
		{
			var textColor = Element.TextColor;

			textView.TextColor = textColor.IsDefault ? NSColor.Black : textColor.ToNSColor();
		}

        void UpdateMaxLength()
        {
            var currentControlText = textView?.Value;

            if (currentControlText != null && currentControlText.Length > Element?.MaxLength)
                textView.Value = currentControlText.Substring(0, Element.MaxLength);
        }

        void UpdateIsReadOnly()
        {
            textView.Editable = !Element.IsReadOnly;
            if (Element.IsReadOnly && Control.Window?.FirstResponder == textView)
                Control.Window?.MakeFirstResponder(null);
        }
    }
}