using System;
namespace Heleus.Apps.Shared
{
	public interface IPointerEventReceiver
	{
		IPointerHandler Handler { get; }
	}
}
