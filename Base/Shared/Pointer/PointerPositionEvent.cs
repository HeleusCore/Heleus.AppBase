using System;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
	public class PointerPositionEvent
	{
		public double StartX
		{
			get;
			private set;
		}

		public double StartY
		{
			get;
			private set;
		}

		public double X
		{
			get;
			private set;
		}

		public double Y
		{
			get;
			private set;
		}

		public double LocalStartX
		{
			get;
			private set;
		}

		public double LocalStartY
		{
			get;
			private set;
		}

		public double LocalX
		{
			get;
			private set;
		}

		public double LocalY
		{
			get;
			private set;
		}

		public PointerPositionEvent(Point startPosition, Point localStartPosition, double x, double y, double localX, double localY)
		{
			StartX = startPosition.X;
			StartY = startPosition.Y;
			X = x;
			Y = y;

			LocalStartX = localStartPosition.X;
			LocalStartY = localStartPosition.Y;
			LocalX = localX;
			LocalY = localY;
		}
	}
}
