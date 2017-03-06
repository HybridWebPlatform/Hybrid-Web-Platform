using System;
using Android.Webkit;

namespace HybridWebControl.Droid
{
	public class WebPlatformChromeClient : WebChromeClient
	{
		Action<IValueCallback, Java.Lang.String, Java.Lang.String> callback;

		public WebPlatformChromeClient(Action<IValueCallback, Java.Lang.String, Java.Lang.String> callback)
		{
			this.callback = callback;
		}

		[Java.Interop.Export]
		public void openFileChooser(IValueCallback uploadMsg, Java.Lang.String acceptType, Java.Lang.String capture)
		{
			if (callback != null)
			{
				callback(uploadMsg, acceptType, capture);
			}
		}

		public override void OnGeolocationPermissionsShowPrompt(string origin, GeolocationPermissions.ICallback callback)
		{
			callback.Invoke(origin, true, false);
		}
	}
}