using System;
using Heleus.Base;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
	public class MovementPointerHandler : IPointerHandler
	{
		Point? startPosition;
		Point? localStartPosition;

		public void PointerDown(double x, double y, double localX, double localY)
		{
			if (startPosition != null)
				return;

			localStartPosition = new Point(localX, localY);
			startPosition = new Point(x, y);

			OnPointerDown?.Invoke(new PointerPositionEvent(startPosition.Value, localStartPosition.Value, x, y, localX, localY));
		}

		public void PointerEnter()
		{
		}

		public void PointerExit()
		{
		}

		public void PointerMoved(double x, double y, double localX, double localY)
		{
			try
			{
				if (startPosition != null && localStartPosition != null)
				{
					OnPointerMoved?.Invoke(new PointerPositionEvent(startPosition.Value, localStartPosition.Value, x, y, localX, localY));
				}
			}
			catch (Exception ex)
			{
                Log.IgnoreException(ex);
			}
		}

		public void PointerUp(bool success)
		{
			OnPointerUp?.Invoke();
			startPosition = null;
			localStartPosition = null;
		}

		public Action<PointerPositionEvent> OnPointerDown;
		public Action OnPointerUp;
		public Action<PointerPositionEvent> OnPointerMoved;
	}
}
