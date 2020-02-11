using System;

namespace Heleus.Apps.Shared
{
	partial class UIApp
	{
		static int activityCount = 0;

		public static bool ShowLoadingIndicator
		{
			set
			{
				if (value)
				{
					if (activityCount == 0)
					{
						ShowLoadingIndicatorNative = true;
					}
					activityCount++;
				}
				else
				{
					activityCount = Math.Max(0, activityCount - 1);
					if (activityCount == 0)
					{
						ShowLoadingIndicatorNative = false;
					}
				}
			}
		}
	}
}