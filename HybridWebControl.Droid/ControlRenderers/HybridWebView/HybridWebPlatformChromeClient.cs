using System;
using Android.Webkit;

namespace HybridWebPlatform.Droid
{
	public class HybridWebPlatformChromeClient : WebChromeClient
	{
		public event Action<Uri> OpenExternalWindow;

		Action<IValueCallback, Java.Lang.String, Java.Lang.String> callback;

		public HybridWebPlatformChromeClient(Action<IValueCallback, Java.Lang.String, Java.Lang.String> callback)
		{
			this.callback = callback;
		}

		[Java.Interop.Export]
		public void openFileChooser(IValueCallback uploadMsg, Java.Lang.String acceptType, Java.Lang.String capture)
		{
			if (callback != null)
			{
				callback(uploadMsg, acceptType, capture);
			}
		}

		public override void OnGeolocationPermissionsShowPrompt(string origin, GeolocationPermissions.ICallback callback)
		{
			callback.Invoke(origin, true, false);
		}

		public override bool OnCreateWindow(WebView view, bool isDialog, bool isUserGesture, Android.OS.Message resultMsg)
		{
			WebView newWebView = new WebView(view.Context);
			newWebView.SetWebViewClient(new HybridWebPlatformNewWindowViewClient(OpenExternalWindow));
			WebView.WebViewTransport transport = (WebView.WebViewTransport)resultMsg.Obj;
			transport.WebView = newWebView;
			resultMsg.SendToTarget();
			return true;
		}
	}
}