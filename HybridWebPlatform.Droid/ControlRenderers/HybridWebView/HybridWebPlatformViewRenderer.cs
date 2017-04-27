using System;
using Android.Content;
using Android.Views;
using HybridWebPlatform;
using HybridWebPlatform.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XLabs.Platform;

[assembly: ExportRenderer(typeof(HybridWebPlatformView), typeof(HybridWebPlatformViewRenderer))]
namespace HybridWebPlatform.Droid
{
	public class HybridWebPlatformViewRenderer : ViewRenderer<HybridWebPlatformView, HybridWebPlatformNativeView>, IHybridWebPlatformActionSource
	{
		public static bool EnableHardwareRendering = false;
		public static bool EnableAdditionalTouchGesturesHandling = true;

		public static Func<HybridWebPlatformViewRenderer, HybridWebPlatformViewClient> GetWebViewClientDelegate;
		public static Func<HybridWebPlatformViewRenderer, HybridWebPlatformChromeClient> GetWebChromeClientDelegate;

		public event Func<Uri, bool> PageLoadRequest;
		public event Action<Uri> PageLoadStarted;
		public event Action<Uri> PageLoadFinished;
		public event Action<Uri, string, int> PageLoadError;
		public event Action<string> JavascriptExecuted;
		public event Action<Uri> PageLoadInNewWindowRequest;

		private HybridWebPlatformViewClient viewClient;
		private const string NativeFuncCall = "Xamarin.call";
		private const string NativeFunction = "function Native(action, data){Xamarin.call(JSON.stringify({ a: action, d: data }));}";

		public bool CanGoBack
		{
			get
			{
				return this.Control.CanGoBack();
			}
		}

		public bool CanGoForward
		{
			get
			{
				return this.Control.CanGoForward();
			}
		}

		public string CurrentUrl
		{
			get
			{
				return this.Control.Url;
			}
		}

		public bool IsLoading
		{
			get
			{
				return this.viewClient.IsLoading;
			}
		}

		public void GoBack()
		{
			this.Control.GoBack();
		}

		public void GoForward()
		{
			this.Control.GoForward();
		}

		public void RefreshPage()
		{
			this.Control.Reload();
		}

		public void LoadPage(Uri page)
		{
			this.Control.LoadUrl(page.AbsoluteUri);
		}

		public void LoadFromString(string html)
		{
			this.Control.LoadData(html, "text/html", "UTF-8");
		}

		public void ExecuteJavascript(string javascript)
		{
			Inject(javascript);
		}

		public override SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			var sizeRequest = base.GetDesiredSize(widthConstraint, heightConstraint);
			sizeRequest.Request = new Size(sizeRequest.Request.Width, 0);
			return sizeRequest;
		}

		protected virtual HybridWebPlatformChromeClient GetWebChromeClient()
		{
			var d = GetWebChromeClientDelegate;

			var client = d != null ? d(this) : new HybridWebPlatformChromeClient(null);

			client.OpenExternalWindow += LoadUrlInExternalWindow;

			return client;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<HybridWebPlatformView> e)
		{
			base.OnElementChanged(e);

			if (this.Control == null && e.NewElement != null)
			{
				var webView = new HybridWebPlatformNativeView(this, EnableAdditionalTouchGesturesHandling);

				webView.Settings.JavaScriptEnabled = true;
				webView.Settings.DomStorageEnabled = true;
				webView.SetLayerType(EnableHardwareRendering ? LayerType.Hardware : LayerType.Software, null);
				webView.SetBackgroundColor(Color.Transparent.ToAndroid());

				this.viewClient = this.CreateWebClient();

				webView.SetWebViewClient(viewClient);
				webView.SetWebChromeClient(this.GetWebChromeClient());

				webView.Settings.JavaScriptEnabled = true;
				webView.Settings.AllowFileAccess = true;
				webView.Settings.AllowContentAccess = true;
				webView.Settings.DatabaseEnabled = true;
				webView.Settings.DomStorageEnabled = true;
				webView.Settings.JavaScriptCanOpenWindowsAutomatically = true;
				webView.Settings.LoadsImagesAutomatically = true;
				webView.Settings.SetGeolocationEnabled(true);
				webView.Settings.SetSupportMultipleWindows(true);
				webView.Settings.SetSupportZoom(false);
				webView.Settings.UserAgentString = "Mozilla/5.0 (Linux; Android 4.0.4; Galaxy Nexus Build/IMM76B) AppleWebKit/535.19 (KHTML, like Gecko) Chrome/18.0.1025.133 Mobile Safari/535.19";

				webView.AddJavascriptInterface(new HybridWebPlatformXamarinApi(this), "Xamarin");

				this.SetNativeControl(webView);

				webView.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
			}

			e.NewElement.SetWebActionSource(this);

		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.Element != null)
			{
				if (this.Control != null)
				{
					this.Control.StopLoading();
				}
			}

			base.Dispose(disposing);
		}

		private void Inject(string script)
		{
			if (Control != null)
			{
				this.Control.LoadUrl(string.Format("javascript: {0}", script));
			}
		}

		private HybridWebPlatformViewClient CreateWebClient()
		{

			var creationDelegate = GetWebViewClientDelegate;

			var webClient = creationDelegate != null ? creationDelegate(this) : new HybridWebPlatformViewClient();

			webClient.ReceivedError += WebClient_ReceivedError;
			webClient.FinishedLoadingUrl += WebClient_FinishedLoadingUrl;
			webClient.StartLoadingUrl += WebClient_StartLoadingUrl;
			webClient.OverrideUrlLoading += WebClient_OverrideUrlLoading;

			return webClient;
		}

		private void LoadUrlInExternalWindow(Uri obj)
		{
			if (PageLoadInNewWindowRequest != null)
			{
				PageLoadInNewWindowRequest(obj);
			}
		}

		private void WebClient_ReceivedError(string arg1, string arg2, int arg3)
		{
			if (PageLoadError != null)
			{
				this.PageLoadError(new Uri(arg1), arg2, arg3);
			}
		}

		private void WebClient_FinishedLoadingUrl(string obj)
		{
			this.Inject(NativeFunction);
			this.Inject(HybridWebPlatformView.GetInitialJsScript(NativeFuncCall));

			if (PageLoadFinished != null)
			{
				this.PageLoadFinished(new Uri(obj));
			}
		}

		private void WebClient_StartLoadingUrl(string obj)
		{
			if (PageLoadStarted != null)
			{
				this.PageLoadStarted(new Uri(obj));
			}
		}

		private bool WebClient_OverrideUrlLoading(string arg)
		{
			Uri page = new Uri(arg);

			if (CheckIfUriIsMailtoOrPhoneNumber(page))
			{
				return true;
			}

			if (PageLoadRequest != null)
			{
				//We negate the result here as if page load request == true we should not override the page request
				return !this.PageLoadRequest(page);
			}
			return false;
		}

		private bool CheckIfUriIsMailtoOrPhoneNumber(Uri uri)
		{
			if (string.Equals(uri.Scheme, "mailto", StringComparison.OrdinalIgnoreCase))
			{
				Intent i = new Intent(Intent.ActionSendto, Android.Net.Uri.Parse(uri.AbsoluteUri));

				i.AddFlags(ActivityFlags.NewTask);

				this.Control.StartActivity(i);

				return true;
			}

			if (string.Equals(uri.Scheme, "tel", StringComparison.OrdinalIgnoreCase))
			{
				Intent i = new Intent(Intent.ActionDial, Android.Net.Uri.Parse(uri.AbsoluteUri));

				i.AddFlags(ActivityFlags.NewTask);

				this.Control.StartActivity(i);

				return true;
			}

			return false;
		}
	}
}
