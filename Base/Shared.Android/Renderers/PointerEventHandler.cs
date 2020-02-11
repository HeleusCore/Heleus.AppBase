using System;
using Android.Views;

namespace Heleus.Apps.Shared.Android.Renderers
{
	public class PointerEventHandlerAndroid : PointerEventHandler
	{
		readonly double density;

		public PointerEventHandlerAndroid()
		{
			var displayMetrics = MainActivity.Current.Resources.DisplayMetrics;
			density = 1.0 / displayMetrics.Density;
		}

		public bool OnTouchEvent(MotionEvent e)
		{
			if (_receiver == null)
				return false;

			//Console.WriteLine(e.Action.ToString());

			if(e.Action == MotionEventActions.Down && e.PointerCount == 1)
			{
				Down(e.RawX * density, e.RawY * density, e.GetX() * density, e.GetY() * density);
				return true;
			}
			else if (e.Action == MotionEventActions.Move)
			{
				Moved(e.RawX * density, e.RawY * density, e.GetX() * density, e.GetY() * density);
				return true;
			}
			else if (e.Action == MotionEventActions.Cancel)
			{
				Up(false);
				return true;
			}
			else if (e.Action == MotionEventActions.Up)
			{
				Up(true);
				return true;
			}

			return false;
		}
	}
}
