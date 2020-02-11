using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Heleus.Base;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
	public enum ThemeMode
	{
		Default,
		Custom
	}

    public enum WindowTheme
    {
        Light,
        Dark
    }

	public class ColorStyle
	{
		public static readonly Dictionary<ThemeColorStyle, ColorStyle> ColorStyles = new Dictionary<ThemeColorStyle, ColorStyle>();

		public bool HasAlpha
		{
			get;
			private set;
		}

		public Color DefaultColor
		{
			get;
			protected set;
		}

		public virtual Color Color
		{
			get
			{
				return DefaultColor;
			}
		}

		public ThemeColorStyle ThemeColorStyle
		{
			get;
			private set;
		}

		public ColorStyle(ThemeColorStyle colorStyle, Color defaultColor, bool hasAlpha = false)
		{
			ThemeColorStyle = colorStyle;
			DefaultColor = defaultColor;
			HasAlpha = hasAlpha;

			//Console.WriteLine ("ColorStyle: " + colorStyle);
			if(colorStyle != ThemeColorStyle.None)
				ColorStyles.Add(colorStyle, this);
		}
	}

	public class ThemedColorStyle : ColorStyle, IPackable, IUnpackable
	{
		public Color DefaultThemedColor
		{
			get;
			private set;
		}

		public Color ThemedColor
		{
			get;
			private set;
		}

		public override Color Color
		{
			get
			{
				if (Theme.ThemeMode == ThemeMode.Custom)
					return ThemedColor;

				return DefaultColor;
			}
		}

		public void UpdateColorStyle(Color color)
		{
			ThemedColor = color;
		}

		public void Pack(Packer writer)
		{
			writer.Pack((byte)(ThemedColor.R * 255));
			writer.Pack((byte)(ThemedColor.G * 255));
			writer.Pack((byte)(ThemedColor.B * 255));
			writer.Pack((byte)(ThemedColor.A * 255));
		}

		public void UnPack(Unpacker reader)
		{
			var r = reader.UnpackByte();
			var g = reader.UnpackByte();
			var b = reader.UnpackByte();
			var a = reader.UnpackByte();

			ThemedColor = Color.FromRgba(r / 255.0, g / 255.0, b / 255.0, a / 255.0);
		}

		public ThemedColorStyle(ThemeColorStyle colorStyle, Color defaultColor, bool hasAlpha = false) : base(colorStyle, defaultColor, hasAlpha)
		{
			ThemedColor = DefaultThemedColor = defaultColor;
		}

		public ThemedColorStyle(ThemeColorStyle colorStyle, Color defaultColor, Color defaultThemedColor, bool hasAlpha = false) : base(colorStyle, defaultColor, hasAlpha)
		{
			ThemedColor = DefaultThemedColor = defaultThemedColor;
		}

		public void SetDefaults(Color defaultColor, Color defaultThemedColor)
		{
			ThemedColor = DefaultThemedColor = defaultThemedColor;
			DefaultColor = defaultColor;
		}
    }

    public class FontStyle
	{
		public static readonly Dictionary<ThemeFontStyle, FontStyle> FontStyles = new Dictionary<ThemeFontStyle, FontStyle>();

		public int DefaultFontSize
		{
			get;
			protected set;
		}

		public virtual int FontSize
		{
			get
			{
				return DefaultFontSize;
			}
		}

		public FontWeight DefaultFontWeight
		{
			get;
			protected set;
		}

		public virtual FontWeight FontWeight
		{
			get
			{
				return DefaultFontWeight;
			}
		}

		public ThemeFontStyle ThemeFontStyle
		{
			get;
			private set;
		}


		public FontStyle(ThemeFontStyle fontStyle, FontWeight fontWeight, int fontSize)
		{
			ThemeFontStyle = fontStyle;
			DefaultFontWeight = fontWeight;
			DefaultFontSize = fontSize;

			//Console.WriteLine ("FontStyle: " + fontStyle);
			if (fontStyle != ThemeFontStyle.None)
				FontStyles.Add(fontStyle, this);
		}
	}

	public class ThemedFontStyle : FontStyle, IPackable, IUnpackable
	{
		public FontWeight ThemedFontWeight
		{
			get;
			private set;
		}

		public override FontWeight FontWeight
		{
			get
			{
				if (Theme.ThemeMode == ThemeMode.Custom)
					return ThemedFontWeight;
				
				return DefaultFontWeight;
			}
		}

		public int ThemedFontSize
		{
			get;
			private set;
		}

		public override int FontSize
		{
			get
			{
				if (Theme.ThemeMode == ThemeMode.Custom)
					return ThemedFontSize;
				
				return DefaultFontSize;
			}
		}

		public void UpdateFontStyle(FontWeight fontWeight, int fontSize)
		{
			ThemedFontWeight = fontWeight;
			ThemedFontSize = fontSize;
		}

		public ThemedFontStyle(ThemeFontStyle fontStyle, FontWeight defaultFontWeight, int defaultFontSize) : base(fontStyle, defaultFontWeight, defaultFontSize)
		{
			ThemedFontWeight = defaultFontWeight;
			ThemedFontSize = defaultFontSize;
		}

		public void SetDefaults(FontWeight defaultFontWeight, int defaultFontSize)
		{
			ThemedFontWeight = DefaultFontWeight = defaultFontWeight;
			ThemedFontSize = DefaultFontSize = defaultFontSize;
		}

		public void Pack(Packer writer)
		{
			writer.Pack((int)ThemedFontWeight);
			writer.Pack(ThemedFontSize);
		}

		public void UnPack(Unpacker reader)
		{
			reader.Unpack(out int tmp);
			ThemedFontWeight = (FontWeight)tmp;

			reader.Unpack(out tmp);
			ThemedFontSize = tmp;
		}
	}

    public interface IButtonStyle
    {
        ColorStyle Color { get; }
        ColorStyle HoverColor { get; }
        ColorStyle HighlightColor { get; }
        ColorStyle DisabledColor { get; }
    }

    class RowButtonStyle : IButtonStyle
    {
        public ColorStyle Color => Theme.RowColor;
        public ColorStyle HoverColor => Theme.RowHoverColor;
        public ColorStyle HighlightColor => Theme.RowHighlightColor;
        public ColorStyle DisabledColor => Theme.RowDisabledColor;
    }

    class SubmitButtonStyle : IButtonStyle
    {
        public ColorStyle Color => Theme.SubmitColor;
        public ColorStyle HoverColor => Theme.SubmitHoverColor;
        public ColorStyle HighlightColor => Theme.SubmitHighlightColor;
        public ColorStyle DisabledColor => Theme.SubmitDisabledColor;
    }

    class CancelButtonStyle : IButtonStyle
    {
        public ColorStyle Color => Theme.CancelColor;
        public ColorStyle HoverColor => Theme.CancelHoverColor;
        public ColorStyle HighlightColor => Theme.CancelHighlightColor;
        public ColorStyle DisabledColor => Theme.CancelDisabledColor;
    }

    static partial class Theme
	{
		static readonly ThemeSettings _themeSettings;

		public static ThemeMode ThemeMode { get; private set; }
		public static int WindowX = 200;
		public static int WindowY = 200;
		public static int WindowWidth = 840;
		public static int WindowHeight = 480;
		public static bool Fullscreen = false;

		static WindowTheme DefaultWindowTheme = WindowTheme.Light;
		static WindowTheme _windowTheme = WindowTheme.Light;
        
		public static WindowTheme WindowTheme
		{
			get
			{
				var theme = _windowTheme;
				if (ThemeMode == ThemeMode.Default)
					theme = DefaultWindowTheme;
				
				return theme;
			}

			set
			{
				_windowTheme = value;
			}
		}

        public static readonly IButtonStyle RowButton = new RowButtonStyle();
        public static readonly IButtonStyle SubmitButton = new SubmitButtonStyle();
        public static readonly IButtonStyle CancelButton = new CancelButtonStyle();

        public static async Task SwitchTheme(ThemeMode mode)
		{
			ThemeMode = mode;
			await PropagateChanges();
		}

		public static async Task PropagateChanges()
		{
			await UIApp.PubSub.PublishAsync(new ThemeChangedEvent());
			Save();
		}

		static void WriteChunks(ChunkWriter writer)
		{
			writer.Write(nameof(ThemeMode), (int)ThemeMode);

			writer.Write(nameof(WindowWidth), WindowWidth);
			writer.Write(nameof(WindowHeight), WindowHeight);
			writer.Write(nameof(Fullscreen), Fullscreen);
			writer.Write(nameof(WindowTheme), (int)_windowTheme);
			writer.Write(nameof(WindowX), WindowX);
			writer.Write(nameof(WindowY), WindowY);

			foreach (var colorStyle in ColorStyle.ColorStyles)
			{
				if (colorStyle.Value is ThemedColorStyle packable)
					writer.Write("Color" + colorStyle.Key.ToString(), packable);
			}

			foreach (var fontStyle in FontStyle.FontStyles)
			{
				if (fontStyle.Value is ThemedFontStyle packable)
					writer.Write("Font" + fontStyle.Key.ToString(), packable);
			}

			Write(writer);
		}

		static void ReadChunks(ChunkReader reader)
		{
			int mode = 0;
			if (reader.Read(nameof(ThemeMode), ref mode))
				ThemeMode = (ThemeMode)mode;

			reader.Read(nameof(WindowWidth), ref WindowWidth);
			reader.Read(nameof(WindowHeight), ref WindowHeight);
			reader.Read(nameof(Fullscreen), ref Fullscreen);
			reader.Read(nameof(WindowX), ref WindowX);
			reader.Read(nameof(WindowY), ref WindowY);

            if (reader.Read(nameof(WindowTheme), ref mode))
				_windowTheme = (WindowTheme)mode;

			foreach (var colorStyle in ColorStyle.ColorStyles)
			{
				if (colorStyle.Value is ThemedColorStyle packable)
					reader.Read("Color" + colorStyle.Key.ToString(), packable);
			}

			foreach (var fontStyle in FontStyle.FontStyles)
			{
				if (fontStyle.Value is ThemedFontStyle packable)
					reader.Read("Font" + fontStyle.Key.ToString(), packable);
			}

			Read(reader);
		}

		class ThemeSettings : NativeSettings
		{
			public ThemeSettings() : base("ThemeData")
			{

			}

			protected override void RestoreSettings()
			{
				try
				{
					var themeData = GetValue("ThemeData", string.Empty);
					if (!string.IsNullOrEmpty(themeData))
					{
						var data = Convert.FromBase64String(themeData);
						ChunkReader.Read(data, ReadChunks);
					}
				}
				catch (Exception ex)
				{
                    Log.IgnoreException(ex);
				}
			}

			protected override void StoreSettings()
			{
				try
				{
					var data = ChunkWriter.Write(WriteChunks);
					if (data != null)
					{
						var themeData = Convert.ToBase64String(data);
						SetValue("ThemeData", themeData);
					}
				}
				catch (Exception ex)
				{
                    Log.IgnoreException(ex);
				}
			}
		}

		static Theme()
		{
			SetDefaults();
			_themeSettings = new ThemeSettings();
		}

		public static void Save()
		{
			_themeSettings?.SaveSettings();
		}
    }
}
