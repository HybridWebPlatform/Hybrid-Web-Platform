using System;
using Android.Webkit;

namespace HybridWebControl.Droid
{
	public class WebPlatformViewClient : WebViewClient
	{
		public event Action<string, string, int> ReceivedError;
		public event Action<string> StartLoadingUrl;
		public event Action<string> FinishedLoadingUrl;
		public event Func<string, bool> OverrideUrlLoading;

		public bool IsLoading
		{
			get;
			private set;
		}

		public override void OnReceivedError(WebView view, ClientError errorCode, string description, string failingUrl)
		{
			IsLoading = false;
			base.OnReceivedError(view, errorCode, description, failingUrl);

			if (ReceivedError != null)
			{
				ReceivedError(failingUrl, description, (int)errorCode);
			}
		}

		public override void OnReceivedError(WebView view, IWebResourceRequest request, WebResourceError error)
		{
			IsLoading = false;
			base.OnReceivedError(view, request, error);

			if (ReceivedError != null)
			{
				ReceivedError(request.Url.ToString(), error.Description, (int)error.ErrorCode);
			}
		}

		public override bool ShouldOverrideUrlLoading(WebView view, string url)
		{
			if (OverrideUrlLoading != null)
			{
				return OverrideUrlLoading(url);
			}
			return false;
		}

		public override void OnPageStarted(WebView view, string url, Android.Graphics.Bitmap favicon)
		{
			IsLoading = true;
			if (StartLoadingUrl != null)
			{
				StartLoadingUrl(url);
			}
		}

		public override void OnPageFinished(WebView view, string url)
		{
			IsLoading = false;
			if (FinishedLoadingUrl != null)
			{
				FinishedLoadingUrl(url);
			}
		}

        public override void OnScaleChanged(WebView view, float oldScale, float newScale)
        {
            base.OnScaleChanged(view, oldScale, newScale);
			if (view != null)
			{
                view.Invalidate();
			}
        }
	}
}
