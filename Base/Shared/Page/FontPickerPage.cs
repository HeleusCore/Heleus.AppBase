using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
	class FontPickerPage : StackPage
	{
		readonly ExtLabel _fontLabel;
		readonly ThemedFontStyle _fontStyle;
		readonly Action<FontWeight, int> _submit;
		readonly SliderRow _sizeRow;
		readonly SelectionRow<FontWeight> _weightRow;

		int _fontSize;
		FontWeight _fontWeight;

		public FontPickerPage(ThemedFontStyle fontStyle, Action<FontWeight, int> action) : base("FontPickerPage")
		{
            AddTitleRow("Title");

            _fontSize = fontStyle.ThemedFontSize;
			_fontWeight = fontStyle.ThemedFontWeight;
			_fontStyle = fontStyle;
			_submit = action;

			AddHeaderRow("FontSection");
			_fontLabel = new ExtLabel
			{
				Text = T("PreviewText"),
				HeightRequest = 80,
				VerticalTextAlignment = TextAlignment.Center,
				HorizontalTextAlignment = TextAlignment.Center,
				BackgroundColor = Theme.RowColor.Color,
				TextColor = Theme.TextColor.Color
			};

			UpdateFontLabel();
			AddView(_fontLabel);
			AddFooterRow();

			AddHeaderRow("FontSize");
			_sizeRow = AddSliderRow(_fontSize, 8, 30);
			_sizeRow.NewValue = (newValue) => {
				_fontSize = newValue;
				UpdateFontLabel();
			};
			AddFooterRow();

			AddHeaderRow("FontWeight");

			var items = new SelectionItemList<FontWeight> {
				new SelectionItem<FontWeight>(FontWeight.Thin, T("Thin")),
				new SelectionItem<FontWeight>(FontWeight.Light, T("Light")),
				new SelectionItem<FontWeight>(FontWeight.Regular, T("Regular")),
				new SelectionItem<FontWeight>(FontWeight.Medium, T("Medium")),
				new SelectionItem<FontWeight>(FontWeight.Bold, T("Bold"))
			};

			_weightRow = AddSelectionRows(items, _fontWeight);
			_weightRow.SelectionChanged = (newValue) => {
				_fontWeight = newValue;
				UpdateFontLabel();
                return Task.CompletedTask;
			};
			AddFooterRow();

			AddSubmitRow("Save", Submit);

			AddHeaderRow();
			var b = AddButtonRow("Default", Reset, true);

			AddFooterRow();
		}

		Task Reset(ButtonRow button)
		{
			_fontWeight = _fontStyle.DefaultFontWeight;
			_weightRow.Selection = _fontWeight;

			_fontSize = _fontStyle.DefaultFontSize;
			_sizeRow.Entry.Text = _fontSize.ToString();
			UpdateFontLabel();

			return Task.FromResult(true);
		}

		async Task Submit(ButtonRow button)
		{
			_submit?.Invoke(_fontWeight, _fontSize);
			await Navigation.PopAsync();
		}

		void UpdateFontLabel()
		{
			_fontLabel.FontSize = _fontSize;
			_fontLabel.FontFamily = FontExtension.GetFontFamily(string.Empty, _fontWeight);
			_fontLabel.FontWeight = _fontWeight;
			_fontLabel.Text = _fontLabel.Text;
		}
	}
}
