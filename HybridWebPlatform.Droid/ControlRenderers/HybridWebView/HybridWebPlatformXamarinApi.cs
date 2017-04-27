using System;
using Android.Webkit;
using Java.Interop;

namespace HybridWebPlatform.Droid
{
	public class HybridWebPlatformXamarinApi : Java.Lang.Object
	{
		private readonly WeakReference<HybridWebPlatformViewRenderer> webHybrid;

		public HybridWebPlatformXamarinApi(HybridWebPlatformViewRenderer webHybrid)
		{
			this.webHybrid = new WeakReference<HybridWebPlatformViewRenderer>(webHybrid);
		}

		[JavascriptInterface]
		[Export("call")]
		public void Call(string message)
		{
			HybridWebPlatformViewRenderer hybrid;
			HybridWebPlatformView webView;
			if (this.webHybrid != null && this.webHybrid.TryGetTarget(out hybrid) && ((webView = hybrid.Element) != null))
			{
				webView.MessageReceived(message);
			}
		}
	}
}
