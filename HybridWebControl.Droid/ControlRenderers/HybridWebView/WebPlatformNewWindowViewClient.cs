using System;
using Android.Webkit;

namespace HybridWebControl.Droid
{
	public class WebPlatformNewWindowViewClient : WebViewClient
	{
		Action<Uri> openExternalLink;

		public WebPlatformNewWindowViewClient(Action<Uri> openExternalLink)
		{
			this.openExternalLink = openExternalLink;
		}

		public override bool ShouldOverrideUrlLoading(WebView view, string url)
		{
			if (openExternalLink != null)
			{
				openExternalLink(new Uri(url));
			}
			view.Dispose();
			return false;
		}
	}
}
