using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
    class ColorPickerPage : StackPage
	{
		Color _color;

		readonly Action<Color> _action;
		readonly SliderRow _redRow;
		readonly SliderRow _blueRow;
		readonly SliderRow _greenRow;
		readonly SliderRow _alphaRow;

		async Task Save (ButtonRow button)
		{
			_action?.Invoke (_color);
			await Navigation.PopAsync ();
		}

		async Task DefaultColor(ButtonRow button)
		{
			var c = (Color)button.Tag;

			_redRow.Entry.Text = ((int)(c.R * 255)).ToString();
			_greenRow.Entry.Text = ((int)(c.G * 255)).ToString();
			_blueRow.Entry.Text = ((int)(c.B * 255)).ToString();

			if(_alphaRow != null)
			{
				_alphaRow.Entry.Text = ((int)(c.A * 255)).ToString();
			}

			await Task.Delay(0);
		}

		public ColorPickerPage(ThemedColorStyle style, Action<Color> action) : this(style.ThemedColor, style.HasAlpha, style.DefaultThemedColor, action)
		{
		}

		public ColorPickerPage(Color baseColor, bool hasAlpha, Color? defaultColor, Action<Color> action) : base("ColorPickerPage")
		{
			_action = action;
			_color = baseColor;

            AddTitleRow("Title");

			AddHeaderRow("ColorSection");

			var box = new ExtBoxView { BackgroundColor = _color };
			box.HeightRequest = 80;
			AddView(box);
			AddFooterRow();

			AddHeaderRow("EntrySection");

			_redRow = AddSliderRow((int)(_color.R * 255), 0, 255);
			_redRow.Entry.TextColor = Color.Red;
			_redRow.NewValue = (int value) =>
			{
				_color = Color.FromRgba(value / 255.0, _color.G, _color.B, _color.A);
				box.BackgroundColor = _color;
			};
			AddView(_redRow);

			_greenRow = AddSliderRow((int)(_color.G * 255), 0, 255);
			_greenRow.Entry.TextColor = Color.Green;
			_greenRow.NewValue = (int value) =>
			{
				_color = Color.FromRgba(_color.R, value / 255.0, _color.B, _color.A);
				box.BackgroundColor = _color;
			};
			AddView(_greenRow);

			_blueRow = AddSliderRow((int)(_color.B * 255), 0, 255);
			_blueRow.Entry.TextColor = Color.Blue;
			_blueRow.NewValue = (int value) =>
			{
				_color = Color.FromRgba(_color.R, _color.G, value / 255.0, _color.A);
				box.BackgroundColor = _color;
			};
			AddView(_blueRow);

			if(hasAlpha)
			{
				_alphaRow = AddSliderRow((int)(_color.A * 255), 0, 255);
				_alphaRow.NewValue = (int value) =>
				{
					_color = Color.FromRgba(_color.R, _color.G, _color.B,  value / 255.0);
					box.BackgroundColor = _color;
				};
			}

			AddFooterRow();

			AddSubmitRow("Save", Save);

			if (defaultColor != null) 
			{
				AddHeaderRow();
				var b = AddButtonRow("Default", DefaultColor, true);
				b.Tag = defaultColor.Value;
				b.SetDetailView(new ExtBoxView { BackgroundColor = defaultColor.Value});

				AddFooterRow();
			}
		}
	}
}

