using System;
using Android.Webkit;

namespace HybridWebPlatform.Droid
{
	public class WebPlatformViewClient : WebViewClient
	{
		public event Action<string, string, int> ReceivedError;
		public event Action<string> StartLoadingUrl;
		public event Action<string> FinishedLoadingUrl;
		public event Func<string, bool> ShouldStartPageLoading;

		public override void OnReceivedError(WebView view, ClientError errorCode, string description, string failingUrl)
		{
			base.OnReceivedError(view, errorCode, description, failingUrl);

			if (ReceivedError != null)
			{
				ReceivedError(failingUrl, description, (int)errorCode);
			}
		}

		public override void OnReceivedError(WebView view, IWebResourceRequest request, WebResourceError error)
		{
			base.OnReceivedError(view, request, error);

			if (ReceivedError != null)
			{
				ReceivedError(request.Url.ToString(), error.Description, (int)error.ErrorCode);
			}
		}

		public override bool ShouldOverrideUrlLoading(WebView view, string url)
		{
			if (ShouldStartPageLoading != null)
			{
				return ShouldStartPageLoading(url);
			}
			return false;
		}

		public override void OnPageStarted(WebView view, string url, Android.Graphics.Bitmap favicon)
		{
			if (StartLoadingUrl != null)
			{
				StartLoadingUrl(url);
			}
		}

		public override void OnPageFinished(WebView view, string url)
		{
			if (FinishedLoadingUrl != null)
			{
				FinishedLoadingUrl(url);
			}
		}
	}
}
