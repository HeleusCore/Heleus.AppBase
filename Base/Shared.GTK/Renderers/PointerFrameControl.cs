using Gdk;
using Gtk;

namespace Heleus.Apps.Shared.GTK.Renderers
{
	public class PointerFrameControl : EventBox
	{
		Color? _backgroundColor;

		readonly IPointerHandler _pointerHandler;

		public PointerFrameControl(IPointerEventReceiver pointerEventReceiver)
		{
			BorderWidth = 0;
			VisibleWindow = false;

			if (pointerEventReceiver?.Handler != null)
			{
				AboveChild = true;
				Events |= EventMask.LeaveNotifyMask
								   | EventMask.EnterNotifyMask
								   | EventMask.ButtonPressMask
								   | EventMask.PointerMotionMask
								   | EventMask.PointerMotionHintMask;

				_pointerHandler = pointerEventReceiver.Handler;
			}
		}

		protected override bool OnButtonPressEvent(EventButton evnt)
		{
			_pointerHandler?.PointerDown(evnt.XRoot, evnt.YRoot, evnt.X, evnt.Y);
			return base.OnButtonPressEvent(evnt);
		}

		protected override bool OnButtonReleaseEvent(EventButton evnt)
		{
			_pointerHandler?.PointerUp(true);
			return base.OnButtonReleaseEvent(evnt);
		}

		protected override bool OnEnterNotifyEvent(EventCrossing evnt)
		{
			_pointerHandler?.PointerEnter();
			return base.OnEnterNotifyEvent(evnt);
		}

		protected override bool OnLeaveNotifyEvent(EventCrossing evnt)
		{
			_pointerHandler?.PointerExit();
			_pointerHandler?.PointerUp(false);

			return base.OnLeaveNotifyEvent(evnt);
		}

		protected override bool OnMotionNotifyEvent(EventMotion evnt)
		{
			_pointerHandler?.PointerMoved(evnt.XRoot, evnt.YRoot, evnt.X, evnt.Y);

			return base.OnMotionNotifyEvent(evnt);
		}

		public void SetBackgroundColor(Color? color)
		{
			_backgroundColor = color;
			if(_backgroundColor.HasValue)
				ModifyBg(StateType.Normal, _backgroundColor.Value);
			QueueDraw();
		}

		public void ResetBackgroundColor()
		{
			QueueDraw();
		}
	}
}
