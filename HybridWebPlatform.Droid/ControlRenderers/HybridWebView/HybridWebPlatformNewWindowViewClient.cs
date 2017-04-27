using System;
using Android.Webkit;

namespace HybridWebPlatform.Droid
{
	public class HybridWebPlatformNewWindowViewClient : WebViewClient
	{
		Action<Uri> openExternalLink;

		public HybridWebPlatformNewWindowViewClient(Action<Uri> openExternalLink)
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
