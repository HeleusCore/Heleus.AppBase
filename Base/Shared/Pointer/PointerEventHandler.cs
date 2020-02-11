using System;
namespace Heleus.Apps.Shared
{
	public class PointerEventHandler
	{
		protected WeakReference<IPointerEventReceiver> _receiver
		{
			get; private set;
		}

		public IPointerEventReceiver Receiver
		{
			set
			{
				if (value == null)
					_receiver = null;
				else
				{
					_receiver = new WeakReference<IPointerEventReceiver>(value);
				}
			}

			get
			{
				if (_receiver != null)
				{
					_receiver.TryGetTarget(out var target);
					return target;
				}
				return null;
			}
		}

		protected void Down(double x, double y, double localX, double localY)
		{
			Receiver?.Handler?.PointerDown(x, y, localX, localY);
		}

		protected void Up(bool success)
		{
			Receiver?.Handler?.PointerUp(success);
		}

		protected void Submit()
		{
			Receiver?.Handler?.PointerUp(true);
		}

		protected void Moved(double x, double y, double localX, double localY)
		{
			Receiver?.Handler?.PointerMoved(x, y, localX, localY);
		}

        protected void Enter()
        {
			Receiver?.Handler?.PointerEnter();
        }

        protected void Exit()
        {
			Receiver?.Handler?.PointerExit();
        }
    }
}
