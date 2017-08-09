using System;
using WebKit;
using UIKit;

namespace HybridWebPlatform.iOS
{
    public class WebPlatformUIDelegate : WKUIDelegate
    {
        public override WKWebView CreateWebView(WKWebView webView, WKWebViewConfiguration configuration, WKNavigationAction navigationAction, WKWindowFeatures windowFeatures)
        {
            if (navigationAction.TargetFrame == null)
                UIApplication.SharedApplication.OpenUrl(navigationAction.Request.Url);
            return null;
        }
    }
}
