using System;
namespace Heleus.Apps.Shared
{
	public interface IPointerHandler
	{
		void PointerDown(double x, double y, double localX, double localY);
		void PointerMoved(double x, double y, double localX, double localY);
		void PointerUp(bool success);

		void PointerEnter();
		void PointerExit();
	}
}
