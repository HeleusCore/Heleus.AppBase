using System;
using System.ComponentModel;
using AppKit;
using Foundation;
using Heleus.Apps.Shared;
using Heleus.Apps.Shared.Apple.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(ExtEntry), typeof(Heleus.Apps.Shared.macOS.Renderers.ExtEntryRenderer2))]

namespace Heleus.Apps.Shared.macOS.Renderers
{
    public class ExtEntryRenderer2 : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ExtEntry> e)
        {
            base.OnElementChanged(e);

            if(Control != null)
            {
                Control.Bezeled = false;
                Control.DrawsBackground = false;
                Control.FocusRingType = NSFocusRingType.None;
                Control.UsesSingleLineMode = true;
            }
        }
    }

	public class BoolEventArgs : EventArgs
	{
		public BoolEventArgs(bool value)
		{
			Value = value;
		}
		public bool Value
		{
			get;
			private set;
		}
	}

	public class FormsNSTextField : NSTextField
	{
		public EventHandler<BoolEventArgs> FocusChanged;
		public override bool ResignFirstResponder()
		{
			FocusChanged?.Invoke(this, new BoolEventArgs(false));
			return base.ResignFirstResponder();
		}
		public override bool BecomeFirstResponder()
		{
			FocusChanged?.Invoke(this, new BoolEventArgs(true));
			return base.BecomeFirstResponder();
		}
		public override void DidEndEditing(Foundation.NSNotification notification)
		{
			base.DidEndEditing(notification);

			if ((NSTextMovement)(notification.UserInfo.ObjectForKey((NSString)FromObject("NSTextMovement")) as NSNumber).Int32Value == NSTextMovement.Return)
			{
				//NextKeyView?.BecomeFirstResponder();
			}
		}
	}

	public class FormsNSSecureTextField : NSSecureTextField
	{
		public EventHandler<BoolEventArgs> FocusChanged;
		public override bool ResignFirstResponder()
		{
			FocusChanged?.Invoke(this, new BoolEventArgs(false));
			return base.ResignFirstResponder();
		}
		public override bool BecomeFirstResponder()
		{
			FocusChanged?.Invoke(this, new BoolEventArgs(true));
			return base.BecomeFirstResponder();
		}
		public override void DidEndEditing(Foundation.NSNotification notification)
		{
			base.DidEndEditing(notification);

			if ((NSTextMovement)(notification.UserInfo.ObjectForKey((NSString)FromObject("NSTextMovement")) as NSNumber).Int32Value == NSTextMovement.Return)
			{
				//NextKeyView?.BecomeFirstResponder();
			}
		}
	}

	public class EntryRenderer : ViewRenderer<ExtEntry, NSTextField>
	{
		bool _disposed;
		NSColor _defaultTextColor;

		IElementController ElementController => Element;

		IEntryController EntryController => Element;

		protected override void OnElementChanged(ElementChangedEventArgs<ExtEntry> e)
		{
			base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    NSTextField textField;
                    if (e.NewElement.IsPassword)
                    {
                        textField = new FormsNSSecureTextField();
                        (textField as FormsNSSecureTextField).FocusChanged += TextFieldFocusChanged;
                    }
                    else
                    {
                        textField = new FormsNSTextField();
                        (textField as FormsNSTextField).FocusChanged += TextFieldFocusChanged;
                    }

                    SetNativeControl(textField);

                    textField.Bezeled = false;
                    textField.DrawsBackground = false;
                    textField.FocusRingType = NSFocusRingType.None;
                    textField.UsesSingleLineMode = true;

                    _defaultTextColor = textField.TextColor;

                    textField.Changed += OnChanged;
                    textField.EditingBegan += OnEditingBegan;
                    textField.EditingEnded += OnEditingEnded;
                }

                if (e.NewElement != null)
                {
                    e.NewElement.ThemeChangedAction = ThemeChanged;
                    UpdatePlaceholder();
                    UpdateText();
                    UpdateColor();
                    UpdateFont();
                    UpdateAlignment();
                }
            }
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Entry.PlaceholderProperty.PropertyName ||
				e.PropertyName == Entry.PlaceholderColorProperty.PropertyName)
				UpdatePlaceholder();
			else if (e.PropertyName == Entry.IsPasswordProperty.PropertyName)
				UpdatePassword();
			else if (e.PropertyName == Entry.TextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Entry.TextColorProperty.PropertyName)
				UpdateColor();
			else if (e.PropertyName == Entry.HorizontalTextAlignmentProperty.PropertyName)
				UpdateAlignment();
			else if (e.PropertyName == Entry.FontAttributesProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Entry.FontFamilyProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Entry.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
			{
				UpdateColor();
				UpdatePlaceholder();
			}

			base.OnElementPropertyChanged(sender, e);
		}

		void ThemeChanged()
		{
		}

		protected override void SetBackgroundColor(Color color)
		{
			if (Control == null)
				return;
			Control.BackgroundColor = color == Color.Default ? NSColor.Clear : color.ToNSColor();

			base.SetBackgroundColor(color);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				if (Control != null)
				{
					Control.EditingBegan -= OnEditingBegan;
					Control.Changed -= OnChanged;
					Control.EditingEnded -= OnEditingEnded;
					var formsNSTextField = (Control as FormsNSTextField);
					if (formsNSTextField != null)
						formsNSTextField.FocusChanged -= TextFieldFocusChanged;
					
					var formsSecureTextField = (Control as FormsNSSecureTextField);
					if (formsSecureTextField != null)
						formsSecureTextField.FocusChanged -= TextFieldFocusChanged;
				}
			}

			base.Dispose(disposing);
		}
		void TextFieldFocusChanged(object sender, BoolEventArgs e)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, e.Value);
		}

		void OnEditingBegan(object sender, EventArgs e)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
		}

		void OnChanged(object sender, EventArgs eventArgs)
		{
			ElementController.SetValueFromRenderer(Entry.TextProperty, Control.StringValue);
		}

		void OnEditingEnded(object sender, EventArgs e)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
			EntryController?.SendCompleted();
		}

		void UpdateAlignment()
		{
			Control.Alignment = Element.HorizontalTextAlignment.ToNativeTextAlignment();
		}

		void UpdateColor()
		{
			var textColor = Element.TextColor;

			if (textColor.IsDefault || !Element.IsEnabled)
				Control.TextColor = _defaultTextColor;
			else
				Control.TextColor = textColor.ToNSColor();
		}

		void UpdatePassword()
		{
			if (Element.IsPassword && (Control is NSSecureTextField))
				return;
			if (!Element.IsPassword && !(Control is NSSecureTextField))
				return;
		}

		void UpdateFont()
		{
			Control.Font = Element.ToNSFont();
		}

		void UpdatePlaceholder()
		{
			var formatted = (FormattedString)Element.Placeholder;

			if (formatted == null)
				return;

			var targetColor = Element.PlaceholderColor;

			// Placeholder default color is 70% gray
			// https://developer.apple.com/library/prerelease/ios/documentation/UIKit/Reference/UITextField_Class/index.html#//apple_ref/occ/instp/UITextField/placeholder

			var color = Element.IsEnabled && !targetColor.IsDefault ? targetColor : NSColor.FromRgba(0.7f, 0.7f, 0.7f, 1).ToColor();

			Control.PlaceholderAttributedString = formatted.ToAttributed(Element, color);
		}

		void UpdateText()
		{
			// ReSharper disable once RedundantCheckBeforeAssignment
			if (Control.StringValue != Element.Text)
				Control.StringValue = Element.Text ?? string.Empty;
		}
	}
}