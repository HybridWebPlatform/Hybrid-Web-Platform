using System;
using Foundation;
using HybridWebPlatform;
using HybridWebPlatform.iOS;
using UIKit;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(HybridWebPlatformView), typeof(HybridWebPlatformViewRenderer))]
namespace HybridWebPlatform.iOS
{
	public class HybridWebPlatformViewRenderer : ViewRenderer<HybridWebPlatformView, WKWebView>, IWKScriptMessageHandler, IHybridWebPlatformActionSource
	{
		public event Func<Uri, bool> PageLoadRequest;
		public event Action<Uri> PageLoadStarted;
		public event Action<Uri> PageLoadFinished;
		public event Action<Uri, string, int> PageLoadError;
		public event Action<string> JavascriptExecuted;
		public event Action<Uri> PageLoadInNewWindowRequest;

		private WKUserContentController userController;

		private const string NativeFuncCall = "window.webkit.messageHandlers.native.postMessage";
		private const string NativeFunction = "function Native(action, data){window.webkit.messageHandlers.native.postMessage(JSON.stringify({ a: action, d: data }));}";

		public bool CanGoBack
		{
			get
			{
				return this.Control.CanGoBack;
			}
		}

		public bool CanGoForward
		{
			get
			{
				return this.Control.CanGoForward;
			}
		}

		public string CurrentUrl
		{
			get
			{
				return this.Control.Url.AbsoluteString;
			}
		}

		public bool IsLoading
		{
			get
			{
				return this.Control.IsLoading;
			}
		}

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return new SizeRequest(Size.Zero, Size.Zero);
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			Control.ScrollView.Frame = Control.Bounds;
		}

		public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
		{
			Element.MessageReceived(message.Body.ToString());
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
			this.Control.LoadRequest(new NSUrlRequest(new NSUrl(page.AbsoluteUri)));
		}

		public void ExecuteJavascript(string javascript)
		{
			Inject(javascript);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<HybridWebPlatformView> e)
		{
			base.OnElementChanged(e);

			if (Control == null && e.NewElement != null)
			{
				userController = new WKUserContentController();

				var config = new WKWebViewConfiguration()
				{
					UserContentController = userController
				};

				var script = new WKUserScript(new NSString(NativeFunction + HybridWebPlatformView.GetInitialJsScript(NativeFuncCall)), WKUserScriptInjectionTime.AtDocumentEnd, false);

				userController.AddUserScript(script);

				userController.AddScriptMessageHandler(this, "native");

				var webView = new WKWebView(Frame, config) { NavigationDelegate = CreateNavidationalDelagate(), UIDelegate = new HybridWebPlatformUIDelegate() };

				webView.Opaque = false;

				webView.BackgroundColor = UIColor.Clear;

				SetNativeControl(webView);

			}

			e.NewElement.SetWebActionSource(this);
		}

		private void Inject(string script)
		{
			if (Control != null)
			{
				InvokeOnMainThread(() => Control.EvaluateJavaScript(new NSString(script), (r, e) =>
				{
					var returnValues = new Tuple<NSObject, NSError>(r, e);
					if (JavascriptExecuted != null)
					{
						JavascriptExecuted("");
					}
				}));
			}
		}

		private HybridWebPlatformNavigationDelegate CreateNavidationalDelagate()
		{
			var navigationDelegate = new HybridWebPlatformNavigationDelegate();

			navigationDelegate.ReceivedError += NavigationDelegate_ReceivedError;
			navigationDelegate.FinishedLoadingUrl += NavigationDelegate_FinishedLoadingUrl;
			navigationDelegate.StartLoadingUrl += NavigationDelegate_StartLoadingUrl;
			navigationDelegate.ShouldStartPageLoading += NavigationDelegate_ShouldStartPageLoading;
			navigationDelegate.OpenExternalWindow += NavigationDelegate_OpenExternalWindow;

			return navigationDelegate;
		}

		private void NavigationDelegate_ReceivedError(string arg1, string arg2, int arg3)
		{
			UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
			if (PageLoadError != null)
			{
				this.PageLoadError(new Uri(arg1), arg2, arg3);
			}
		}

		private void NavigationDelegate_FinishedLoadingUrl(string obj)
		{
			UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
			if (PageLoadFinished != null)
			{
				this.PageLoadFinished(new Uri(obj));
			}
		}

		private void NavigationDelegate_StartLoadingUrl(string obj)
		{
			UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
			if (PageLoadStarted != null)
			{
				this.PageLoadStarted(new Uri(obj));
			}
		}

		private bool NavigationDelegate_ShouldStartPageLoading(string arg)
		{
			if (PageLoadRequest != null)
			{
				return this.PageLoadRequest(new Uri(arg));
			}
			return true;
		}

		private void NavigationDelegate_OpenExternalWindow(Uri obj)
		{
			UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
			if (PageLoadInNewWindowRequest != null)
			{
				this.PageLoadInNewWindowRequest(obj);
			}
		}
	}
}
