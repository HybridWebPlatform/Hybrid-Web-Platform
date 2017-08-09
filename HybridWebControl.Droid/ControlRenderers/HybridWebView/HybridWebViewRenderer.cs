﻿using System;
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
using HybridWebControl.Droid;
using HybridWebControl;
using XLabs.Platform;
using Android.OS;

[assembly: ExportRenderer(typeof(HybridWebView), typeof(HybridWebViewRenderer))]
namespace HybridWebControl.Droid
{
	public class HybridWebViewRenderer : ViewRenderer<HybridWebView, WebPlatformNativeView>, IHybridWebViewActionSource
	{
		public static bool EnableAdditionalTouchGesturesHandling = true;

		public static Func<HybridWebViewRenderer, WebPlatformViewClient> GetWebViewClientDelegate;
		public static Func<HybridWebViewRenderer, WebPlatformChromeClient> GetWebChromeClientDelegate;

		public event Func<Uri, bool> PageLoadRequest;
		public event Action<Uri> PageLoadStarted;
		public event Action<Uri> PageLoadFinished;
		public event Action<Uri, string, int> PageLoadError;
		public event Action<string> JavascriptExecuted;
		public event Action<Uri> PageLoadInNewWindowRequest;

        private WebPlatformViewClient viewClient;
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

        public string UserAgent 
        { 
            get
            {
                return this.Control.Settings.UserAgentString;
            }
            set
            {
                this.Control.Settings.UserAgentString = value;
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

		protected virtual WebPlatformChromeClient GetWebChromeClient()
		{
			var d = GetWebChromeClientDelegate;

			var client = d != null ? d(this) : new WebPlatformChromeClient(null);

			client.OpenExternalWindow += LoadUrlInExternalWindow;

			return client;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<HybridWebView> e)
		{
			base.OnElementChanged(e);

			if (this.Control == null && e.NewElement != null)
			{
				var webView = new WebPlatformNativeView(this, EnableAdditionalTouchGesturesHandling);

                SetCookieToBrowser(e.NewElement);

				webView.Settings.JavaScriptEnabled = true;
				webView.Settings.DomStorageEnabled = true;
				webView.SetLayerType(LayerType.Hardware, null);
				// HACK: Fix blinking on Android 4 with hardware acceleration
                // see details at https://stackoverflow.com/questions/9476151/webview-flashing-with-white-background-if-hardware-acceleration-is-enabled-an
				bool isSupportHardwareRenderingAndNotBlink = Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop;
                if (isSupportHardwareRenderingAndNotBlink)
                {
					webView.SetBackgroundColor(Color.Transparent.ToAndroid());
				}
                else
                {
                    webView.SetBackgroundColor(Color.FromRgba(0, 0, 0, 1).ToAndroid());
                }

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

        private void SetCookieToBrowser(HybridWebView webView)
        {
            if (!webView.IsCookieSetRequested)
                return;

			var cookieManager = CookieManager.Instance;
			cookieManager.SetAcceptCookie(true);

			DateTime expiresDate = DateTime.UtcNow.AddSeconds(HybridWebView.NativeAppCookieExpiresInS);
			string expiresHttpDate = expiresDate.ToString("r");

            // TODO: Warning! SetCookie is async, but it's impossible to use 
            // callback parameter now because it doesn't support in Android 4
            // See details at https://developer.android.com/reference/android/webkit/CookieManager.html#setCookie(java.lang.String, java.lang.String, android.webkit.ValueCallback<java.lang.Boolean>)
            cookieManager.SetCookie(
                webView.FutureLoadedPageCookieHost,
                $"{webView.FutureLoadedPageCookieName}={webView.FutureLoadedPageCookieValue}; Expires={expiresHttpDate}; Path=/; "
            );
        }

		private void Inject(string script)
		{
			if (Control != null)
			{
				this.Control.LoadUrl(string.Format("javascript: {0}", script));
			}
		}

		private WebPlatformViewClient CreateWebClient()
		{

			var creationDelegate = GetWebViewClientDelegate;

			var webClient = creationDelegate != null ? creationDelegate(this) : new WebPlatformViewClient();

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
			this.Inject(HybridWebView.GetInitialJsScript(NativeFuncCall));

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
			else if (string.Equals(uri.Scheme, "tel", StringComparison.OrdinalIgnoreCase))
			{
				Intent i = new Intent(Intent.ActionDial, Android.Net.Uri.Parse(uri.AbsoluteUri));

				i.AddFlags(ActivityFlags.NewTask);

				this.Control.StartActivity(i);

				return true;
			}
			else if (string.Equals(uri.Scheme, "sms", StringComparison.OrdinalIgnoreCase))
			{
				Intent i = new Intent(Intent.ActionSendto, Android.Net.Uri.Parse(uri.AbsoluteUri));

				i.AddFlags(ActivityFlags.NewTask);

				this.Control.StartActivity(i);

				return true;
			}

			return false;
		}
    }
}
