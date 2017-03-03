using System;
using Android.Webkit;
using HybridWebPlatform.HybridWeb;
using Java.Interop;

namespace HybridWebPlatform.Droid
{
	public class WebPlatformXamarinApi : Java.Lang.Object
	{
		private readonly WeakReference<HybridWebViewRenderer> webHybrid;

		public WebPlatformXamarinApi(HybridWebViewRenderer webHybrid)
		{
			this.webHybrid = new WeakReference<HybridWebViewRenderer>(webHybrid);
		}

		[JavascriptInterface]
		[Export("call")]
		public void Call(string message)
		{
			HybridWebViewRenderer hybrid;
			HybridWebView webView;
			if (this.webHybrid != null && this.webHybrid.TryGetTarget(out hybrid) && ((webView = hybrid.Element) != null))
			{
				webView.MessageReceived(message);
			}
		}
	}
}
