using System;
using Android.Webkit;
using Java.Lang;

namespace HybridWebControl.Droid
{
	public class WebPlatformJavascriptCallback : Java.Lang.Object, IValueCallback
	{
		public event Action<string> ReceivedCallback;

		public WebPlatformJavascriptCallback()
		{
		}

		public void OnReceiveValue(Java.Lang.Object value)
		{
			if (ReceivedCallback != null)
			{
				ReceivedCallback(((Java.Lang.String)value).ToString());
			}
		}
	}
}
