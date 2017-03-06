using System;
using Android.Runtime;
using Android.Views;
using Android.Webkit;

namespace HybridWebControl.Droid
{
	public class WebPlatformNativeView : WebView
	{
		private readonly GestureDetector detector;
		private readonly bool enableDetector;

		public WebPlatformNativeView(HybridWebViewRenderer renderer, bool enableAdditionalTouchDetector) : base(renderer.Context)
		{
			enableDetector = enableAdditionalTouchDetector;

			if (enableDetector)
			{
				var listener = new MyGestureListener(renderer);
				this.detector = new GestureDetector(this.Context, listener);
			}
		}

		public WebPlatformNativeView(IntPtr ptr, JniHandleOwnership handle) : base(ptr, handle)
		{

		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			if (enableDetector)
			{
				this.detector.OnTouchEvent(e);
			}
			return base.OnTouchEvent(e);
		}

		private class MyGestureListener : GestureDetector.SimpleOnGestureListener
		{
			private const int SWIPE_MIN_DISTANCE = 120;
			private const int SWIPE_MAX_OFF_PATH = 200;
			private const int SWIPE_THRESHOLD_VELOCITY = 200;

			private readonly WeakReference<HybridWebViewRenderer> webHybrid;

			public MyGestureListener(HybridWebViewRenderer renderer)
			{
				this.webHybrid = new WeakReference<HybridWebViewRenderer>(renderer);
			}

			//                public override void OnLongPress(MotionEvent e)
			//                {
			//                    Console.WriteLine("OnLongPress");
			//                    base.OnLongPress(e);
			//                }
			//
			//                public override bool OnDoubleTap(MotionEvent e)
			//                {
			//                    Console.WriteLine("OnDoubleTap");
			//                    return base.OnDoubleTap(e);
			//                }
			//
			//                public override bool OnDoubleTapEvent(MotionEvent e)
			//                {
			//                    Console.WriteLine("OnDoubleTapEvent");
			//                    return base.OnDoubleTapEvent(e);
			//                }
			//
			//                public override bool OnSingleTapUp(MotionEvent e)
			//                {
			//                    Console.WriteLine("OnSingleTapUp");
			//                    return base.OnSingleTapUp(e);
			//                }
			//
			//                public override bool OnDown(MotionEvent e)
			//                {
			//                    Console.WriteLine("OnDown");
			//                    return base.OnDown(e);
			//                }

			//public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
			//{
			//HybridWebPlatformRenderer hybrid;

			//if (this.webHybrid.TryGetTarget(out hybrid) && Math.Abs(velocityX) > SWIPE_THRESHOLD_VELOCITY)
			//{
			//	if (e1.GetX() - e2.GetX() > SWIPE_MIN_DISTANCE)
			//	{
			//		hybrid.Element.OnLeftSwipe(this, EventArgs.Empty);
			//	}
			//	else if (e2.GetX() - e1.GetX() > SWIPE_MIN_DISTANCE)
			//	{
			//		hybrid.Element.OnRightSwipe(this, EventArgs.Empty);
			//	}
			//}

			//return base.OnFling(e1, e2, velocityX, velocityY);
			//}

			//                public override bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
			//                {
			//                    Console.WriteLine("OnScroll");
			//                    return base.OnScroll(e1, e2, distanceX, distanceY);
			//                }
			//
			//                public override void OnShowPress(MotionEvent e)
			//                {
			//                    Console.WriteLine("OnShowPress");
			//                    base.OnShowPress(e);
			//                }
			//
			//                public override bool OnSingleTapConfirmed(MotionEvent e)
			//                {
			//                    Console.WriteLine("OnSingleTapConfirmed");
			//                    return base.OnSingleTapConfirmed(e);
			//                }

		}
	}
}