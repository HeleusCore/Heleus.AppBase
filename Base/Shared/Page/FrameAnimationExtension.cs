using System;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
	public static class FrameAnimationExtension
	{
		static Random r = new Random();

		public static void HighlightAnimation(this View frame)
		{
			var w = 1.0;
			if (frame.Width > 0) // normalize for a specific button width
				w = 360.0 / frame.Width;

			frame.RotateXTo((5 + r.NextDouble() * -10) * (r.Next(2) == 0 ? 1 : -1) * w, 60);
			frame.RotateYTo(5 + r.NextDouble() * 5 * w, 60);
		}

		public static void ResetAnimation(this View frame)
		{
			frame.RotateXTo(0, 100);
			frame.RotateYTo(0, 100);
		}
	}
}
