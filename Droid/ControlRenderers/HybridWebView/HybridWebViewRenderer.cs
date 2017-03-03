using System;
using System.Text;
using System.Text.RegularExpressions;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Java.Interop;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Object = Java.Lang.Object;
using WebView = Android.Webkit.WebView;
using System.Threading.Tasks;
using System.Threading;
using HybridWebPlatform.HybridWeb;
using HybridWebPlatform.HybridWeb.Contracts;
using HybridWebPlatform.Droid;
using HybridWebPlatform.HybridWeb.Utilites;

[assembly: ExportRenderer(typeof(HybridWebView), typeof(HybridWebViewRenderer))]
namespace HybridWebPlatform.Droid
{
	public class HybridWebViewRenderer : ViewRenderer<HybridWebView, WebPlatformNativeView>, HybridWeb.IWebPlatformActionSource
	{
		public static bool EnableHardwareRendering = false;
		public static bool EnableAdditionalTouchGesturesHandling = true;

		public static Func<HybridWebViewRenderer, WebPlatformViewClient> GetWebViewClientDelegate;
		public static Func<HybridWebViewRenderer, WebPlatformChromeClient> GetWebChromeClientDelegate;

		public event Func<Uri, bool> PageLoadRequest;
		public event Action<Uri> PageLoadStarted;
		public event Action<Uri> PageLoadFinished;
		public event Action<Uri, string, int> PageLoadError;
		public event Action<string> JavascriptExecuted;

		private WebPlatformViewClient webClient;
		private WebPlatformJavascriptCallback javascriptCallback;

		private const string NativeFuncCall = "Xamarin.call";
		private const string NativeFunction = "function Native(action, data){Xamarin.call(JSON.stringify({ a: action, d: data }));}";
		private const string FuncFormat = "^(file|http|https)://(local|LOCAL)/Func(=|%3D)(?<CallbackIdx>[\\d]+)(&|%26)(?<FuncName>[\\w]+)/";

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

		public override SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			var sizeRequest = base.GetDesiredSize(widthConstraint, heightConstraint);
			sizeRequest.Request = new Size(sizeRequest.Request.Width, 0);
			return sizeRequest;
		}

		private WebPlatformViewClient WebViewClient
		{
			get
			{
				if (webClient == null)
				{
					var creationDelegate = GetWebViewClientDelegate;

					webClient = creationDelegate != null ? creationDelegate(this) : new WebPlatformViewClient();

					webClient.ReceivedError += WebClient_ReceivedError;
					webClient.FinishedLoadingUrl += WebClient_FinishedLoadingUrl;
					webClient.StartLoadingUrl += WebClient_StartLoadingUrl;
					webClient.ShouldStartPageLoading += WebClient_ShouldStartPageLoading;
				}

				return webClient;
			}
		}

		private WebPlatformJavascriptCallback JavascriptCallback
		{
			get
			{
				if (javascriptCallback == null)
				{
					javascriptCallback = new WebPlatformJavascriptCallback();
					javascriptCallback.ReceivedCallback += JavascriptExecuted;
				}

				return javascriptCallback;
			}
		}

		protected virtual WebPlatformChromeClient GetWebChromeClient()
		{
			var d = GetWebChromeClientDelegate;

			return d != null ? d(this) : new WebPlatformChromeClient(null);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<HybridWebView> e)
		{
			base.OnElementChanged(e);

			if (this.Control == null && e.NewElement != null)
			{
				var webView = new WebPlatformNativeView(this, EnableAdditionalTouchGesturesHandling);

				webView.Settings.JavaScriptEnabled = true;
				webView.Settings.DomStorageEnabled = true;
				webView.SetLayerType(EnableHardwareRendering ? LayerType.Hardware : LayerType.Software, null);
				webView.SetBackgroundColor(Color.Transparent.ToAndroid());

				webView.SetWebViewClient(this.WebViewClient);
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

				webView.AddJavascriptInterface(new WebPlatformXamarinApi(this), "Xamarin");

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

		public void ExecuteJavascript(string javascript)
		{
			Inject(javascript);
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
			this.Inject(HybridWebView.GetInitialJsScript(NativeFuncCall));

			if (PageLoadFinished != null)
			{
				this.PageLoadFinished(new Uri(obj));
			}
		}

		private void Inject(string script)
		{
			if (Control != null)
			{
				this.Control.LoadUrl(string.Format("javascript: {0}", script));
				//this.Control.EvaluateJavascript(script, JavascriptCallback);
			}
		}

		private void WebClient_StartLoadingUrl(string obj)
		{
			if (PageLoadStarted != null)
			{
				this.PageLoadStarted(new Uri(obj));
			}
		}

		private bool WebClient_ShouldStartPageLoading(string arg)
		{
			if (PageLoadRequest != null)
			{
				return this.PageLoadRequest(new Uri(arg));
			}
			return false;
		}

		private void TryInvoke(string function, string data)
		{
			Action<string> action;

			if (this.Element != null && this.Element.TryGetAction(function, out action))
			{
				action.Invoke(data);
			}
			else
			{
				System.Diagnostics.Debug.WriteLine("Unhandled callback {0} was called from JavaScript", function);
			}
		}
	}
}
