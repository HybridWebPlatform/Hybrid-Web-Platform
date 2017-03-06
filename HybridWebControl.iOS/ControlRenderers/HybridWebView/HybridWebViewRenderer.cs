using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Foundation;
using HybridWebPlatform.iOS;
using UIKit;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using System.Threading.Tasks;
using System.Threading;
using HybridWebControl;
using HybridWebControl.iOS;

[assembly: ExportRenderer(typeof(HybridWebView), typeof(HybridWebViewRenderer))]
namespace HybridWebControl.iOS
{
	public class HybridWebViewRenderer : ViewRenderer<HybridWebView, WKWebView>, IWKScriptMessageHandler, IHybridWebViewActionSource
	{
		public event Func<Uri, bool> PageLoadRequest;
		public event Action<Uri> PageLoadStarted;
		public event Action<Uri> PageLoadFinished;
		public event Action<Uri, string, int> PageLoadError;
		public event Action<string> JavascriptExecuted;

		//private UISwipeGestureRecognizer _leftSwipeGestureRecognizer;
		//private UISwipeGestureRecognizer _rightSwipeGestureRecognizer;
		private WKUserContentController userController;
		private WebPlatformNavigationDelegate navigationDelegate;

		private static object syncContext = new object();

		private const string NativeFuncCall = "window.webkit.messageHandlers.native.postMessage";
		private const string NativeFunction = "function Native(action, data){window.webkit.messageHandlers.native.postMessage(JSON.stringify({ a: action, d: data }));}";
		private const string FuncFormat = "^(file|http|https)://(local|LOCAL)/Func(=|%3D)(?<CallbackIdx>[\\d]+)(&|%26)(?<FuncName>[\\w]+)/";
		private static readonly Regex FuncExpression = new Regex(FuncFormat);

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

		private WebPlatformNavigationDelegate WebNavigationDelegate
		{
			get
			{
				if (navigationDelegate == null)
				{
					navigationDelegate = new WebPlatformNavigationDelegate();

					navigationDelegate.ReceivedError += NavigationDelegate_ReceivedError;
					navigationDelegate.FinishedLoadingUrl += NavigationDelegate_FinishedLoadingUrl;
					navigationDelegate.StartLoadingUrl += NavigationDelegate_StartLoadingUrl;
					navigationDelegate.ShouldStartPageLoading += NavigationDelegate_ShouldStartPageLoading;
				}

				return navigationDelegate;
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

		protected override void OnElementChanged(ElementChangedEventArgs<HybridWebView> e)
		{
			base.OnElementChanged(e);

			if (Control == null && e.NewElement != null)
			{
				userController = new WKUserContentController();

				var config = new WKWebViewConfiguration()
				{
					UserContentController = userController
				};

				var script = new WKUserScript(new NSString(NativeFunction + HybridWebView.GetInitialJsScript(NativeFuncCall)), WKUserScriptInjectionTime.AtDocumentEnd, false);

				userController.AddUserScript(script);

				userController.AddScriptMessageHandler(this, "native");

				var webView = new WKWebView(Frame, config) { NavigationDelegate = WebNavigationDelegate };

				SetNativeControl(webView);

				//_leftSwipeGestureRecognizer = new UISwipeGestureRecognizer(() => Element.OnLeftSwipe(this, EventArgs.Empty))
				//{
				//	Direction = UISwipeGestureRecognizerDirection.Left
				//};

				//_rightSwipeGestureRecognizer = new UISwipeGestureRecognizer(() => Element.OnRightSwipe(this, EventArgs.Empty))
				//{
				//	Direction = UISwipeGestureRecognizerDirection.Right
				//};

				//webView.AddGestureRecognizer(_leftSwipeGestureRecognizer);
				//webView.AddGestureRecognizer(_rightSwipeGestureRecognizer);
			}

			if (e.NewElement == null && Control != null)
			{
				//Control.RemoveGestureRecognizer(_leftSwipeGestureRecognizer);
				//Control.RemoveGestureRecognizer(_rightSwipeGestureRecognizer);
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

		private void NavigationDelegate_ReceivedError(string arg1, string arg2, int arg3)
		{
			if (PageLoadError != null)
			{
				this.PageLoadError(new Uri(arg1), arg2, arg3);
			}
		}

		private void NavigationDelegate_FinishedLoadingUrl(string obj)
		{
			if (PageLoadFinished != null)
			{
				this.PageLoadFinished(new Uri(obj));
			}
		}

		private void NavigationDelegate_StartLoadingUrl(string obj)
		{
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
	}
}
