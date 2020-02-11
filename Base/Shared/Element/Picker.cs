using System;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
	public class ExtPicker : Picker, IThemeable
	{
		public Action ThemeChangedAction = null;

		ColorStyle colorStyle;
		public ColorStyle ColorStyle
		{
			get
			{
				return colorStyle;
			}

			set
			{
				colorStyle = value;
				if (colorStyle != null)
					TextColor = colorStyle.Color;
			}
		}

		FontStyle fontStyle;
		public FontStyle FontStyle
		{
			get
			{
				return fontStyle;
			}

			set
			{
				fontStyle = value;
				if (this.ThemeUseFontWeight() && fontStyle != null)
					FontFamily = FontExtension.GetFontFamily(FontFamily, fontStyle.FontWeight);
				if (this.ThemeUseFontSize() && fontStyle != null)
					FontSize = fontStyle.FontSize;
			}
		}

		public virtual void ThemeChanged()
		{
			ColorStyle = colorStyle;
			
			ThemeChangedAction?.Invoke();
		}
	}
	
	public class ExtPicker<T> : ExtPicker
	{

		SelectionItemList<T> pickerItems;

		public new SelectionItemList<T> Items
		{
			get
			{
				return pickerItems;
			}

			set
			{
				pickerItems = value;
				UpdateItems();
			}
		}

		void UpdateItems()
		{
			base.Items.Clear();
			if (Items != null)
			{
				foreach (var item in Items)
				{
					base.Items.Add(item.Description);
				}
			}
		}

		public T Selection
		{
			get
			{
				if (Items == null)
					return default(T);

				if (SelectedIndex == -1)
					return default(T);
				
				var index = base.Items[SelectedIndex];
				foreach (var item in Items)
				{
					if (item.Description == index)
						return item.Key;
				}

				return default(T);
			}

			set
			{
				if (Items == null)
					return;

				string description = null;
				foreach(var item in Items)
				{
					if(item.Key.Equals(value))
					{
						description = item.Description;
						break;
					}
				}

				for (int i = 0; i < base.Items.Count; i++)
				{
					if (base.Items[i] == description)
					{
						SelectedIndex = i;
						return;
					}
				}
			}
		}
	}
}
