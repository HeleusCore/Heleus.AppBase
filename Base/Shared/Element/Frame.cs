using System;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
	public class ExtFrame : ContentView, IThemeable
	{
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
				{
					BackgroundColor = colorStyle.Color;
				}
			}
		}

		public virtual void ThemeChanged()
		{
			ColorStyle = colorStyle;
			Content?.PropagateThemeChange();
		}
	}

	public enum PointerFrameType
	{
		None,
		Button,
		Movement
	}

	public class PointerFrame : ExtFrame, IPointerEventReceiver
	{
		public IPointerHandler Handler
		{
			get;
			protected set;
		}

		public ButtonPointerHandler PointerHandler => Handler as ButtonPointerHandler;
        public MovementPointerHandler MovementHandler => Handler as MovementPointerHandler;

		public PointerFrame()
		{
			Padding = new Thickness(0);
		}

		public void SetPointerFrameType(PointerFrameType pointerFrameType)
		{
			switch(pointerFrameType)
			{
				case PointerFrameType.Button:
					Handler = new ButtonPointerHandler();
					break;
				case PointerFrameType.Movement:
					Handler = new MovementPointerHandler();
					break;
				default:
					Handler = null;
					break;
			}
		}
	}
}
