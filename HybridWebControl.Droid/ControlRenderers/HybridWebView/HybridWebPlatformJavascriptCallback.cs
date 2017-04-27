using System;
using Android.Webkit;
using Java.Lang;

namespace HybridWebPlatform.Droid
{
	public class HybridWebPlatformJavascriptCallback : Java.Lang.Object, IValueCallback
	{
		public event Action<string> ReceivedCallback;

		public HybridWebPlatformJavascriptCallback()
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
