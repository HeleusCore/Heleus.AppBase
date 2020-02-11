using System;
using Heleus.Base;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
	public class ButtonPointerHandler : IPointerHandler
	{
		protected Point? touchPosition
		{
			get; private set;
		}

#if WP
        Windows.UI.Xaml.DispatcherTimer startTimer = null;
        Windows.UI.Xaml.DispatcherTimer endTimer = null;
#else
        System.Timers.Timer startTimer;
		System.Timers.Timer endTimer;
#endif

        ~ButtonPointerHandler()
        {
            ClearStopTimer();
            ClearEndTimer();
        }

		void ClearStopTimer()
		{
			if (startTimer != null)
			{
				try
				{
#if WP
                    startTimer.Tick -= StartElapsed;
                    startTimer.Stop();
#else
                    startTimer.Elapsed -= StartElapsed;
					startTimer.Stop();
					startTimer.Close();
					startTimer.Dispose();
#endif
                    startTimer = null;
				}
                catch(Exception ex)
				{
                    Log.IgnoreException(ex);
				}
			}
		}

		void ClearEndTimer()
		{
			if (endTimer != null)
			{
				try
				{
#if WP
                    endTimer.Tick -= EndElapsed;
                    endTimer.Stop();
#else
                    endTimer.Elapsed -= EndElapsed;
					endTimer.Stop();
					endTimer.Close();
					endTimer.Dispose();
#endif
                    endTimer = null;
                }
                catch (Exception ex)
				{
                    Log.IgnoreException(ex);
				}
			}
		}

		public void PointerDown(double x, double y, double localX, double localY)
		{
			touchPosition = new Point(x, y);

			ClearEndTimer();

			if (startTimer == null)
			{
#if WP
                startTimer = new Windows.UI.Xaml.DispatcherTimer();
                startTimer.Interval = TimeSpan.FromMilliseconds(200);
                startTimer.Tick += StartElapsed;
#else
                startTimer = new System.Timers.Timer(200);
				startTimer.Elapsed += StartElapsed;
#endif
				startTimer.Start();
			}
		}

		public void PointerUp(bool success)
		{
			if (touchPosition == null)
				return;
			
			touchPosition = null;

			ClearStopTimer();

			if (success)
			{
#if WP
                endTimer = new Windows.UI.Xaml.DispatcherTimer();
                endTimer.Interval = TimeSpan.FromMilliseconds(300);
                endTimer.Tick += EndElapsed;
#else
				endTimer = new System.Timers.Timer(300);
				endTimer.Elapsed += EndElapsed;
#endif
				endTimer.Start();

				OnPointerDown?.Invoke();

				if(UIApp.IsMacOS)
				{
					Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
                	{
						OnPointerAction?.Invoke();
                    	return false;
                	});
				}
				else
					OnPointerAction?.Invoke();
			}
			else
			{
				OnPointerUp?.Invoke();
			}
		}

		public void PointerMoved(double x, double y, double localX, double localY)
		{
			if (touchPosition != null)
			{
				//Console.WriteLine("Move X: " + x + " Y: " + y);

				var p = touchPosition.Value;

				var yDistance = Math.Abs(y - p.Y);
				var xDistance = Math.Abs(x - p.X);

				//Console.WriteLine("Distance X: " + xDistance + " Y: " + yDistance);

				if (yDistance > 4 || xDistance > 20)
					PointerUp(false);
			}
		}

#if WP
        void StartElapsed(object sender, object e)
#else
        void StartElapsed(object sender, System.Timers.ElapsedEventArgs e)
#endif
        {
            ClearStopTimer();
			UIApp.Run(() => OnPointerDown?.Invoke());
		}

#if WP
        void EndElapsed(object sender, object e)
#else
        void EndElapsed(object sender, System.Timers.ElapsedEventArgs e)
#endif
        {
            ClearEndTimer();
			UIApp.Run(() => OnPointerUp?.Invoke());
		}

		public void TouchMove()
		{
		}

		public void PointerEnter()
		{
			OnPointerEnter?.Invoke();
		}

		public void PointerExit()
		{
			OnPointerExit?.Invoke();
		}

		public Action OnPointerAction;
		public Action OnPointerDown;
		public Action OnPointerUp;

        public Action OnPointerEnter;
        public Action OnPointerExit;
	}
}
