using System;
using WebKit;
using UIKit;

namespace HybridWebPlatform.iOS
{
    public class WebPlatformUIDelegate : WKUIDelegate
    {
        public Action<Uri> OpenExternalWindow;

        public override WKWebView CreateWebView(WKWebView webView, WKWebViewConfiguration configuration, WKNavigationAction navigationAction, WKWindowFeatures windowFeatures)
        {
            if (navigationAction.TargetFrame == null)
            {
                if (OpenExternalWindow != null)
                {
                    OpenExternalWindow(new Uri(navigationAction.Request.Url.AbsoluteString));
                }
            }
            return null;
        }
    }
}
