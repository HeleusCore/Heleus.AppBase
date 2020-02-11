using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
    public class StackRow : PointerFrame
    {
        public static double DisabledAlphaValue = 0.2;

        public Action<ColorStyle> OnColorStyleChange;

        public void ColorStyleChanged()
        {
            OnColorStyleChange?.Invoke(ColorStyle);
        }

        public readonly Layout RowLayout;

        protected IButtonStyle _rowStyle;

        public IButtonStyle RowStyle
        {
            get => _rowStyle;

            set
            {
                _rowStyle = value;
                if (_rowStyle == null)
                {
                    BackgroundColor = Color.Transparent;
                    return;
                }

                if (IsEnabled)
                    ColorStyle = _rowStyle.Color;
                else
                    ColorStyle = _rowStyle.DisabledColor;
            }
        }

        public virtual new bool IsEnabled
        {
            get => base.IsEnabled;

            set
            {
                if (base.IsEnabled && value)
                    return;
                if (!base.IsEnabled && !value)
                    return;

                base.IsEnabled = value;

                if (_rowStyle != null)
                {
                    if (value)
                    {
                        ColorStyle = _rowStyle.Color;
                        ColorStyleChanged();
                    }
                    else
                    {
                        ColorStyle = _rowStyle.DisabledColor;
                        ColorStyleChanged();
                    }
                }
            }
        }

        public StackRow(Layout rowLayout)
        {
            Content = RowLayout = rowLayout;
            var inputTransparent = (UIApp.IsMacOS);

            if (inputTransparent)
                RowLayout.InputTransparent = true;

            LayoutChanged += Layouted;

            if (UIApp.IsAndroid)
                BackgroundColor = Color.Transparent;
        }

        private void Layouted(object sender, EventArgs e)
        {
            Layouted((int)Width);
        }

        protected virtual void Layouted(int width)
        {

        }

        public string Identifier { get; set; }
        public object Tag { get; set; }
    }

    public class TextRow : StackRow
    {
        public readonly ExtLabel Label = new ExtLabel();
        public new AbsoluteLayout RowLayout => base.RowLayout as AbsoluteLayout;

        public TextRow() : base(new AbsoluteLayout())
        {
            Label.InputTransparent = true;
            Label.FontStyle = Theme.TextFont;
            Label.ColorStyle = Theme.TextColor;
            Label.Margin = new Thickness(15, 0, 15, 0);

            Label.WidthRequest = Screen.StackPageWidth - 30;

            RowLayout.Children.Add(Label);
        }

        protected override void Layouted(int width)
        {
            if ((int)Label.WidthRequest != (width - 30))
                Label.WidthRequest = Width - 30;
        }
    }

    public enum HeaderRowType
    {
        Title,
        Header,
        Footer,
        Invisible
    }

    public class HeaderRow : StackRow
    {
        public readonly ExtLabel Label = new ExtLabel();
        public new AbsoluteLayout RowLayout => base.RowLayout as AbsoluteLayout;

        public HeaderRow(HeaderRowType rowType) : base(new AbsoluteLayout())
        {
            InputTransparent = true;
            Label.InputTransparent = true;

            Label.ColorStyle = Theme.TextColor;

            if (rowType == HeaderRowType.Title)
            {
                Label.Margin = new Thickness(10, 5, 0, 5);
                Label.FontStyle = Theme.RowTitleFont;
            }
            else if (rowType == HeaderRowType.Header)
            {
                Label.Margin = new Thickness(5, 5, 0, 0);
                Label.FontStyle = Theme.RowHeaderFont;
            }
            else
            {
                Label.Margin = new Thickness(5, 0, 0, 0);
                Label.FontStyle = Theme.RowFooterFont;
            }

            Label.WidthRequest = Screen.StackPageWidth - 30;

            if (rowType != HeaderRowType.Invisible)
                RowLayout.Children.Add(Label);
        }

        protected override void Layouted(int width)
        {
            if ((int)Label.WidthRequest != (width - 30))
                Label.WidthRequest = Width - 30;
        }
    }

    public class SeparatorRow : StackRow
    {
        public const int DefaultMobileMinimumHeight = 46;
        public const int DefaultDesktopMinimumHeight = 38;

        public static int DefaultMinimumHeight
        {
            get
            {
                return UIApp.IsDesktop ? DefaultDesktopMinimumHeight : DefaultMobileMinimumHeight;
            }
        }

        View _baseView;
        Thickness _baseViewMargin;

        public View DetailView
        {
            get;
            private set;
        }

        public override bool IsEnabled
        {
            get => base.IsEnabled;

            set
            {
                if (base.IsEnabled && value)
                    return;
                if (!base.IsEnabled && !value)
                    return;

                base.IsEnabled = value;

                if (value)
                {
                    if (DetailView is ExtLabel dv)
                        dv.ColorStyleAlpha = 1.0;
                }
                else
                {
                    if (DetailView is ExtLabel dv)
                        dv.ColorStyleAlpha = DisabledAlphaValue;
                }
            }
        }

        public new AbsoluteLayout RowLayout => base.RowLayout as AbsoluteLayout;

        public SeparatorRow(AbsoluteLayout rowLayout = null, int minHeight = DefaultMobileMinimumHeight) : base(rowLayout ?? new AbsoluteLayout())
        {
            if (minHeight == DefaultMobileMinimumHeight && UIApp.IsDesktop)
                minHeight = DefaultDesktopMinimumHeight;

            // force minimum height
            RowLayout.Children.Add(new ContentView { HeightRequest = minHeight, WidthRequest = 0, InputTransparent = true });

            if (UIApp.IsMacOS)
            {
                if (GetType() != typeof(PointerFrame))
                {
                    InputTransparent = true;
                }
            }

            RowStyle = Theme.RowButton;
        }

        public SeparatorRow(AbsoluteLayout rowLayout, bool translucent, int minHeight = DefaultMobileMinimumHeight) : this(rowLayout, minHeight)
        {
            if (!translucent)
            {
                RowStyle = null;
            }
        }

        protected void SetDetailView(View view, View original, int viewSize = 20)
        {
            _baseView = original;
            _baseViewMargin = original.Margin;

            original.Margin = new Thickness(viewSize + 30, original.Margin.Top, original.Margin.Right, original.Margin.Bottom);

            view.InputTransparent = true;
            view.Margin = new Thickness(15, 0, 0, 0);
            view.WidthRequest = view.HeightRequest = viewSize;
            AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.YProportional);
            AbsoluteLayout.SetLayoutBounds(view, new Rectangle(0, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
            RowLayout.Children.Insert(0, view);

            DetailView = view;
        }

        public void RemoveDetailView()
        {
            if (DetailView != null)
            {
                _baseView.Margin = _baseViewMargin;

                RowLayout.Children.Remove(DetailView);
                DetailView = null;
            }
        }

        public FontIcon SetDetailViewIcon(char icon, int viewSize = 25)
        {
            return SetDetailViewIcon(icon, IconSet.Light, viewSize);
        }

        public FontIcon SetDetailViewIcon(char icon, IconSet iconSet, int viewSize = 25)
        {
            var FontIcon = new FontIcon
            {
                InputTransparent = true,
                FontStyle = Theme.RowIconFont,
                ColorStyle = Theme.TextColor
            };

            FontIcon.ThemeUseFontWeight(false);
            FontIcon.SetIcon(iconSet, icon);
            //FontIcon.BackgroundColor = Color.Red;


            SetDetailView(FontIcon, viewSize);

            FontIcon.Margin = new Thickness(10, 0, 0, 0);

            if (!UIApp.IsUWP || !UIApp.IsUWP)
                FontIcon.WidthRequest = (int)(viewSize * 1.75);
            if (UIApp.IsUWP)
                FontIcon.WidthRequest = FontIcon.HeightRequest = (int)(viewSize * 1.5);

            return FontIcon;
        }

        public virtual void SetDetailView(View view, int viewSize)
        {
        }
    }

    public class ViewRow : StackRow
    {
        public readonly View View;
        public new AbsoluteLayout RowLayout => base.RowLayout as AbsoluteLayout;

        public T GetView<T>() where T : View
        {
            return View as T;
        }

        public ViewRow(View view, bool addMargin, AbsoluteLayout rowLayout = null) : base(rowLayout ?? new AbsoluteLayout())
        {
            View = view;

            if (addMargin)
            {
                view.Margin = new Thickness(15, 15, 15, 15);
            }

            RowStyle = Theme.RowButton;

            AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.SizeProportional);
            AbsoluteLayout.SetLayoutBounds(view, new Rectangle(0, 0, 1, 1));

            RowLayout.Children.Clear();
            RowLayout.Children.Add(view);
        }
    }

    public class ImageRow : SeparatorRow
    {
        double _aspect;

        public int MaxWidth = 350;

        public readonly ExtImage ImageView = new ExtImage();
        public readonly AbsoluteLayout ImageLayout = new AbsoluteLayout();

        public double Aspect
        {
            get => _aspect;

            set
            {
                _aspect = value;
                if (Width <= 0)
                    return;

                var width = Width;// Math.Min(MaxWidth, (int)Width - 30);
                ImageLayout.WidthRequest = width;
                ImageLayout.HeightRequest = width * _aspect;
            }
        }

        public ImageRow(double aspect)
        {
            _aspect = aspect;

            ImageView.Aspect = Xamarin.Forms.Aspect.AspectFill;

            AbsoluteLayout.SetLayoutFlags(ImageView, AbsoluteLayoutFlags.SizeProportional);
            AbsoluteLayout.SetLayoutBounds(ImageView, new Rectangle(0, 0, 1, 1));

            ImageLayout.Children.Add(ImageView);

            ImageLayout.Margin = new Thickness(0);
            AbsoluteLayout.SetLayoutFlags(ImageLayout, AbsoluteLayoutFlags.XProportional);
            AbsoluteLayout.SetLayoutBounds(ImageLayout, new Rectangle(0.5, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));

            RowLayout.Children.Add(ImageLayout);
            SizeChanged += (sender, evt) =>
            {
                Aspect = _aspect;
            };
        }

        public override void SetDetailView(View view, int viewSize)
        {
        }
    }

    public class LabelRow : SeparatorRow
    {
        public readonly ExtLabel Label = new ExtLabel();

        public int LabelPadding = 40;

        public override bool IsEnabled
        {
            get => base.IsEnabled;

            set
            {
                if (base.IsEnabled && value)
                    return;
                if (!base.IsEnabled && !value)
                    return;

                base.IsEnabled = value;

                if (value)
                {
                    Label.ColorStyleAlpha = 1.0;
                }
                else
                {
                    Label.ColorStyleAlpha = DisabledAlphaValue;
                }
            }
        }

        public LabelRow(AbsoluteLayout rowLayout = null) : base(rowLayout)
        {
            Label.InputTransparent = true;
            if (!UIApp.IsGTK)
                Label.IsEnabled = false;

            Label.FontStyle = Theme.RowFont;
            Label.ColorStyle = Theme.TextColor;

            Label.Margin = new Thickness(15, 5, 0, 5);

            Label.WidthRequest = Screen.StackPageWidth - (LabelPadding + Label.Margin.Left);

            AbsoluteLayout.SetLayoutFlags(Label, AbsoluteLayoutFlags.YProportional);
            AbsoluteLayout.SetLayoutBounds(Label, new Rectangle(0, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));

            RowLayout.Children.Add(Label);
        }

        protected override void Layouted(int width)
        {
            base.Layouted(width);

            var p = (int)(LabelPadding + Label.Margin.Left);

            if ((int)Label.WidthRequest != (width - p))
                Label.WidthRequest = Width - p;
        }

        public override void SetDetailView(View view, int viewSize = 25)
        {
            SetDetailView(view, Label, viewSize);
        }

        public void SetMultilineText(string text, string detail)
        {
            var textSpan = new Span { Text = text }.SetStyle(Theme.TextFont, Theme.TextColor);
            var detailSpan = new Span { Text = $"\n{detail}" }.SetStyle(Theme.MicroFont, Theme.TextColor);

            Label.Margin = new Thickness(Label.Margin.Left, 2, Label.Margin.Right, 2);

            var formatted = new FormattedString();
            formatted.Spans.Add(textSpan);
            formatted.Spans.Add(detailSpan);

            Label.FormattedText = formatted;
        }
    }

    public class IconLabelRow : LabelRow
    {
        public readonly FontIcon FontIcon = new FontIcon();

        public override bool IsEnabled
        {
            get => base.IsEnabled;

            set
            {
                if (base.IsEnabled && value)
                    return;
                if (!base.IsEnabled && !value)
                    return;

                base.IsEnabled = value;

                if (value)
                {
                    FontIcon.ColorStyleAlpha = 1.0;
                }
                else
                {
                    FontIcon.ColorStyleAlpha = DisabledAlphaValue;
                }
            }
        }

        public IconLabelRow(char arrowIcon, AbsoluteLayout rowLayout = null) : base(rowLayout)
        {
            FontIcon.InputTransparent = true;
            if (!UIApp.IsGTK)
                FontIcon.IsEnabled = false;

            FontIcon.ThemeUseFontWeight(false);
            FontIcon.FontStyle = Theme.RowIconFont;
            FontIcon.ColorStyle = Theme.TextColor;

            FontIcon.Margin = new Thickness(0, 0, 10, 0);
            FontIcon.Icon = arrowIcon;

            //FontIcon.WidthRequest = FontIcon.HeightRequest = 35;

            AbsoluteLayout.SetLayoutFlags(FontIcon, AbsoluteLayoutFlags.PositionProportional);
            RowLayout.Children.Add(FontIcon, new Point(1, 0.5));
        }
    }

    public class IconRow : SeparatorRow
    {
        public readonly FontIcon FontIcon = new FontIcon();

        public new AbsoluteLayout RowLayout => base.RowLayout as AbsoluteLayout;

        public override bool IsEnabled
        {
            get => base.IsEnabled;

            set
            {
                if (base.IsEnabled && value)
                    return;
                if (!base.IsEnabled && !value)
                    return;

                base.IsEnabled = value;

                if (value)
                {
                    FontIcon.ColorStyleAlpha = 1.0;
                }
                else
                {
                    FontIcon.ColorStyleAlpha = DisabledAlphaValue;
                }
            }
        }

        public IconRow(char arrowIcon, AbsoluteLayout rowLayout = null) : base(rowLayout ?? new AbsoluteLayout())
        {
            RowStyle = Theme.RowButton;

            FontIcon.InputTransparent = true;
            if (!UIApp.IsGTK)
                FontIcon.IsEnabled = false;

            FontIcon.ThemeUseFontWeight(false);
            FontIcon.FontStyle = Theme.RowIconFont;
            FontIcon.ColorStyle = Theme.TextColor;

            FontIcon.Margin = new Thickness(0, 0, 10, 0);
            FontIcon.Icon = arrowIcon;

            //FontIcon.WidthRequest = FontIcon.HeightRequest = 35;

            AbsoluteLayout.SetLayoutFlags(FontIcon, AbsoluteLayoutFlags.PositionProportional);
            RowLayout.Children.Add(FontIcon, new Point(1, 0.5));
        }
    }


    public class ButtonRowHandler<T> where T : StackRow
    {
        public Func<T, Task> ButtonAction;

        readonly StackRow _stackRow;
        bool _inside;

        void PointerDown()
        {
            if (_stackRow.IsEnabled)
            {
                //this.HighlightAnimation();
                if (_stackRow.RowStyle != null)
                {
                    _stackRow.ColorStyle = _stackRow.RowStyle.HighlightColor;
                    _stackRow.ColorStyleChanged();
                }
            }
        }

        void PointerUp()
        {
            //this.ResetAnimation();

            if (!_stackRow.IsEnabled)
                return;

            if (_inside)
            {
                if (_stackRow.RowStyle != null)
                {
                    _stackRow.ColorStyle = _stackRow.RowStyle.HoverColor;
                    _stackRow.ColorStyleChanged();
                }
            }
            else
            {
                if (_stackRow.RowStyle != null)
                {
                    _stackRow.ColorStyle = _stackRow.RowStyle.Color;
                    _stackRow.ColorStyleChanged();
                }
            }
        }

        void PointerEnter()
        {
            if (_stackRow.IsEnabled)
            {
                if (_stackRow.RowStyle != null)
                {
                    _stackRow.ColorStyle = _stackRow.RowStyle.HoverColor;
                    _stackRow.ColorStyleChanged();
                }
                _inside = true;
            }
        }

        void PointerExit()
        {
            if (!_stackRow.IsEnabled)
                return;

            if (_stackRow.RowStyle != null)
            {
                _stackRow.ColorStyle = _stackRow.RowStyle.Color;
                _stackRow.ColorStyleChanged();
            }
            _inside = false;
        }

        public void ResetColorStyle()
        {
            PointerExit();
        }

        public ButtonRowHandler(StackRow stackRow)
        {
            _stackRow = stackRow;

            _stackRow.SetPointerFrameType(PointerFrameType.Button);
            var handler = _stackRow.PointerHandler;

            handler.OnPointerDown = PointerDown;
            handler.OnPointerUp = PointerUp;

            handler.OnPointerEnter = PointerEnter;
            handler.OnPointerExit = PointerExit;

            handler.OnPointerAction = () =>
            {
                if (!_stackRow.IsEnabled)
                    return;

                UIApp.Run(async () =>
                {
                    if (ButtonAction != null)
                        await ButtonAction.Invoke(_stackRow as T);
                });
            };
        }
    }

    public class ButtonRow : IconLabelRow
    {
        readonly ButtonRowHandler<ButtonRow> _buttonHandler;
        public Func<ButtonRow, Task> ButtonAction { get => _buttonHandler.ButtonAction; set => _buttonHandler.ButtonAction = value; }

        public void ResetColorStyle()
        {
            _buttonHandler.ResetColorStyle();
        }

        public ButtonRow(char icon, Func<ButtonRow, Task> action, AbsoluteLayout rowLayout = null) : base(icon, rowLayout)
        {
            _buttonHandler = new ButtonRowHandler<ButtonRow>(this)
            {
                ButtonAction = action
            };
        }
    }

    public class ButtonLayoutRow : IconRow
    {
        readonly ButtonRowHandler<ButtonLayoutRow> _buttonHandler;
        public Func<ButtonLayoutRow, Task> ButtonAction { get => _buttonHandler.ButtonAction; set => _buttonHandler.ButtonAction = value; }

        public void ResetColorStyle()
        {
            _buttonHandler.ResetColorStyle();
        }

        public ButtonLayoutRow(char icon, Func<ButtonLayoutRow, Task> action, AbsoluteLayout rowLayout) : base(icon, rowLayout)
        {
            _buttonHandler = new ButtonRowHandler<ButtonLayoutRow>(this)
            {
                ButtonAction = action
            };
        }
    }

    public class ButtonViewRow<T> : IconRow where T : View
    {
        public readonly T View;

        public new AbsoluteLayout RowLayout => base.RowLayout as AbsoluteLayout;

        readonly ButtonRowHandler<ButtonViewRow<T>> _buttonHandler;
        public Func<ButtonViewRow<T>, Task> ButtonAction { get => _buttonHandler.ButtonAction; set => _buttonHandler.ButtonAction = value; }

        public void ResetColorStyle()
        {
            _buttonHandler.ResetColorStyle();
        }

        public ButtonViewRow(char icon, Func<ButtonViewRow<T>, Task> action, T view, bool addMargin, AbsoluteLayout rowLayout = null) : base(icon, rowLayout)
        {
            View = view;
            _buttonHandler = new ButtonRowHandler<ButtonViewRow<T>>(this)
            {
                ButtonAction = action
            };

            if (addMargin)
            {
                view.Margin = new Thickness(15, 15, 35, 15);
            }

            AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.SizeProportional);
            AbsoluteLayout.SetLayoutBounds(view, new Rectangle(0, 0, 1, 1));

            RowLayout.Children.Add(view);
        }
    }

    public class EntryRow : IconLabelRow
    {
        public readonly ExtEntry Edit = new ExtEntry();

        public override bool IsEnabled
        {
            get => base.IsEnabled;
            set
            {
                base.IsEnabled = Edit.IsEnabled = value;
            }
        }

        public EntryRow(char icon) : base(icon)
        {
            Edit.Margin = new Thickness(15, 0, 15, 0);

            //if (!App.IsGTK)
            {
                Edit.TextColorStyle = Theme.TextColor;
                Edit.FontStyle = Theme.RowFont;

                if (UIApp.IsGTK)
                    Edit.BackgroundColor = Theme.RowColor.Color;
            }

            Edit.PlaceholderColor = Color.Gray;

            FontIcon.IsVisible = false;

            AbsoluteLayout.SetLayoutFlags(Edit, AbsoluteLayoutFlags.YProportional | AbsoluteLayoutFlags.WidthProportional);
            AbsoluteLayout.SetLayoutBounds(Edit, new Rectangle(0, 0.5, 1, AbsoluteLayout.AutoSize));

            RowLayout.Children.Add(Edit);
        }

        public override void SetDetailView(View view, int viewSize = 25)
        {
            SetDetailView(view, Edit, viewSize);
        }
    }

    public class EditorRow : SeparatorRow
    {
#if GTK // Editor is broken, hangs on Linux
        public readonly ExtEntry Edit = new ExtEntry();
#else
        public readonly ExtEditor Edit = new ExtEditor();
#endif
        public override bool IsEnabled
        {
            get => base.IsEnabled;
            set
            {
                base.IsEnabled = Edit.IsEnabled = value;
            }
        }

        public EditorRow()
        {
            Edit.Margin = new Thickness(15, 5, 15, 5);
#if !GTK
            Edit.AutoSize = EditorAutoSizeOption.TextChanges;
            Edit.HeightRequest = 6 * Theme.RowFont.FontSize;
#endif
            //if (!App.IsGTK)
            {
                Edit.TextColorStyle = Theme.TextColor;
                Edit.FontStyle = Theme.RowFont;

                if (UIApp.IsGTK)
				    Edit.BackgroundColor = Theme.RowColor.Color;
			}

			AbsoluteLayout.SetLayoutFlags(Edit, AbsoluteLayoutFlags.SizeProportional);
			AbsoluteLayout.SetLayoutBounds(Edit, new Rectangle(0, 0, 1, 1));

			RowLayout.Children.Add(Edit);
		}

		public override void SetDetailView(View view, int viewSize = 25)
		{
			SetDetailView(view, Edit, viewSize);
		}
	}

	public class PickerRow<T> : IconLabelRow
	{
		public readonly ExtPicker<T> Picker = new ExtPicker<T> ();
		
		public PickerRow(char icon) : base(icon)
		{
			Picker.FontStyle = Theme.RowFont;
			Picker.ColorStyle = Theme.TextColor;

			Picker.Margin = new Thickness(15, 5, 50, 5);

			AbsoluteLayout.SetLayoutFlags(Picker, AbsoluteLayoutFlags.YProportional);
			AbsoluteLayout.SetLayoutBounds(Picker, new Rectangle(0, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));

			RowLayout.Children.Add(Picker);
		}
	}

	public class SliderRow : SeparatorRow
	{
		int MinValue = 0;
		int MaxValue = 255;

		public readonly ExtSlider Slider = new ExtSlider();
		public readonly ExtEntry Entry = new ExtEntry();

        public override bool IsEnabled
        {
            get => base.IsEnabled;
            set
            {
                base.IsEnabled = Slider.IsEnabled = Entry.IsEnabled = value;
            }
        }

        public Action<int> NewValue;

		public SliderRow(int value, int minValue, int maxValue)
		{
			MinValue = minValue;
			MaxValue = maxValue;

			Entry.WidthRequest = 60;
			Entry.HorizontalTextAlignment = TextAlignment.End;
			Entry.Margin = new Thickness(15, 0, 0, 0);
			Entry.Text = value.ToString();
			Entry.TextChanged += Entry_TextChanged;
			Entry.FontStyle = Theme.RowFont;
			Entry.TextColorStyle = Theme.TextColor;

			AbsoluteLayout.SetLayoutFlags(Entry, AbsoluteLayoutFlags.YProportional);
			AbsoluteLayout.SetLayoutBounds(Entry, new Rectangle(0, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));

			Slider.Margin = new Thickness(Entry.WidthRequest + 30, 0, 15, 0);
			Slider.Minimum = 0;
			Slider.Maximum = MaxValue - MinValue;
			Slider.Value = value - minValue;
			Slider.ValueChanged += Slider_ValueChanged;


			AbsoluteLayout.SetLayoutFlags(Slider, AbsoluteLayoutFlags.WidthProportional | AbsoluteLayoutFlags.YProportional);
			AbsoluteLayout.SetLayoutBounds(Slider, new Rectangle(0, 0.5, 1, AbsoluteLayout.AutoSize));

			RowLayout.Children.Add(Slider);
			RowLayout.Children.Add(Entry);
		}

		void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
		{
			var v = (int)e.NewValue + MinValue;
			Entry.Text = v.ToString();

			NewValue?.Invoke(v);
		}

		void Entry_TextChanged(object sender, TextChangedEventArgs e)
		{
			var text = e.NewTextValue;
			if (!string.IsNullOrEmpty(text))
			{
				if (!int.TryParse(e.NewTextValue, out var result))
				{
					Entry.Text = e.OldTextValue;
				}
				else
				{
					if (result < MinValue)
					{
						Entry.Text = MinValue.ToString();
						Slider.Value = 0;
						NewValue?.Invoke(MinValue);

					}
					else if (result > MaxValue)
					{
						Entry.Text = MaxValue.ToString();
						Slider.Value = MaxValue - MinValue;
						NewValue?.Invoke(MaxValue);
					}
					else
					{
						Slider.Value = result - MinValue;
						NewValue?.Invoke(result);
					}
				}
			}
		}

		public override void SetDetailView(View view, int viewSize = 25)
		{
		}
	}

	public class SimpleSliderRow : SeparatorRow
	{
		int MinValue = 0;
		int MaxValue = 255;

		public ExtSlider Slider = new ExtSlider();

		public Action<int> NewValue;

        public override bool IsEnabled
        {
            get => base.IsEnabled;
            set
            {
                base.IsEnabled = Slider.IsEnabled = value;
            }
        }

        public SimpleSliderRow(int value, int minValue, int maxValue)
		{
			MinValue = minValue;
			MaxValue = maxValue;

			Slider.Minimum = 0;
			Slider.Maximum = MaxValue - MinValue;
			Slider.Value = value - minValue;
			Slider.ValueChanged += Slider_ValueChanged;


			AbsoluteLayout.SetLayoutFlags(Slider, AbsoluteLayoutFlags.WidthProportional | AbsoluteLayoutFlags.YProportional);
			AbsoluteLayout.SetLayoutBounds(Slider, new Rectangle(0, 0.5, 1, AbsoluteLayout.AutoSize));

			RowLayout.Children.Add(Slider);
		}

		void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
		{
			var v = (int)e.NewValue + MinValue;

			NewValue?.Invoke(v);
		}

		public override void SetDetailView(View view, int viewSize = 25)
		{
		}
	}

	public class SwitchRow<T> : ButtonRow where T : ExtSwitch, new()
	{
		public T Switch = new T();

		public override bool IsEnabled
		{
            get => base.IsEnabled;
			set
			{
				base.IsEnabled = Switch.IsEnabled = value;
			}
		}

		public SwitchRow(char icon) : base(icon, null)
		{
			PointerHandler.OnPointerAction = () =>
			{
				if (Switch.IsEnabled)
					Switch.IsToggled = !Switch.IsToggled;
			};

			RowLayout.Children.Remove(FontIcon);

			Switch.Margin = new Thickness(0, 0, 10, 0);
			AbsoluteLayout.SetLayoutFlags(Switch, AbsoluteLayoutFlags.PositionProportional);

			RowLayout.Children.Add(Switch, new Point(1, 0.5));
		}
	}

	public class SwitchRow : SwitchRow<ExtSwitch>
	{
		public SwitchRow(char icon) : base(icon)
		{

		}
	}

	public class SelectionItem<T>
	{
		public T Key
		{
			get;
			private set;
		}

		public string Description
		{
			get;
			private set;
		}

		public SelectionItem(T key, string description)
		{
			Key = key;
			Description = description;
		}
	}

	public class SelectionItemList<T> : List<SelectionItem<T>>
	{
		
	}

	public class SelectionRow<T> : ButtonRow
	{
		T _select;
		public T Selection
		{
			get
			{
				return _select;
			}

			set
			{
				var old = GetButton(_select);
				if (old != null)
				{
					old.FontIcon.IsVisible = false;
				}

				var button = GetButton(value);
				if (button != null)
				{
					button.FontIcon.IsVisible = true;
				}

				_select = value;
                if(SelectionChanged != null)
                    UIApp.Run(() => SelectionChanged.Invoke(_select));
			}
		}

		public SelectionItem<T> SelectionItem
		{
			get
			{
				var button = GetButton(_select);
				if (button != null)
					return button.Tag as SelectionItem<T>;
				return null;
			}

			set
			{
				if (value != null)
					Selection = value.Key;
				else
					Selection = default(T);
			}
		}

        public readonly char Icon;
		public Func<T, Task> SelectionChanged;
        public IReadOnlyList<ButtonRow> Buttons { get; private set; }

		ButtonRow GetButton(T key)
		{
			foreach (var b in Buttons)
			{
				if (b.Tag is SelectionItem<T> item && item.Key.Equals(key))
				{
					return b;
				}
			}

			return null;
		}

		async Task Select(ButtonRow row)
		{
			var item = (SelectionItem<T>)row.Tag;

            if (SelectionItem == item)
                return;

			Selection = item.Key;
            if(SelectionChanged != null)
			    await SelectionChanged.Invoke(item.Key);

            return;
		}

		public SelectionRow(char icon) : base(icon, null)
		{
            ButtonAction = Select;
            Icon = icon;
		}

        public void SetupSelectionRow(StackPage page, IEnumerable<SelectionItem<T>> items)
        {
            var buttons = new List<ButtonRow>();

            var oldAddIndex = page.AddIndex;

            var first = true;
            foreach (var item in items)
            {
                ButtonRow button;
                if (first)
                {
                    first = false;
                    Label.Text = item.Description;
                    page.AddView(this);
                    button = this;
                }
                else
                {
                    button = page.AddButtonRow(item.Description, Select);
                }

                page.AddIndex = button;

                button.FontIcon.Icon = Icon;
                button.FontIcon.IsVisible = false;
                button.Tag = item;
                buttons.Add(button);
            }

            page.AddIndex = oldAddIndex;
            Buttons = buttons;
        }
	}
}
