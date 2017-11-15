using System;
using System.Linq;
using WebKit;
using UIKit;

namespace HybridWebPlatform.iOS
{
    public class WebPlatformUIDelegate : WKUIDelegate
    {
        public Func<Uri, bool> ShouldOpenExternalWindow;

        public override WKWebView CreateWebView(WKWebView webView, WKWebViewConfiguration configuration, WKNavigationAction navigationAction, WKWindowFeatures windowFeatures)
        {
			var newWebView = new WKWebView(new CoreGraphics.CGRect(0, 0, 0, 0), configuration);

            if (navigationAction.TargetFrame == null)
            {
                if (ShouldOpenExternalWindow != null)
                {
                    if (ShouldOpenExternalWindow(new Uri(navigationAction.Request.Url.AbsoluteString)))
                    {
                        newWebView = new WKWebView(webView.Frame, configuration);
                        newWebView.UIDelegate = this;
                        webView.AddSubview(newWebView);
                    }
                }
            }

            return newWebView;
        }

        public override void DidClose(WKWebView webView)
        {
            webView.RemoveFromSuperview();
        }

#region Handling JavaScript alerts
        public override void RunJavaScriptAlertPanel(WKWebView webView, string message, WKFrameInfo frame, Action completionHandler)
        {
            var alertController = UIAlertController.Create("", message, UIAlertControllerStyle.Alert);

            var actionOk = UIAlertAction.Create("Ok", UIAlertActionStyle.Default, (UIAlertAction obj) =>
            {
                completionHandler();
            });

            alertController.AddAction(actionOk);

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alertController, true, null);
        }

        public override void RunJavaScriptConfirmPanel(WKWebView webView, string message, WKFrameInfo frame, Action<bool> completionHandler)
        {
            var alertController = UIAlertController.Create("", message, UIAlertControllerStyle.Alert);

            var actionOk = UIAlertAction.Create("Ok", UIAlertActionStyle.Default, (UIAlertAction obj) =>
            {
                completionHandler(true);
            });
            var actionCancel = UIAlertAction.Create("Cancel", UIAlertActionStyle.Default, (UIAlertAction obj) =>
             {
                 completionHandler(false);
             });

            alertController.AddAction(actionOk);
            alertController.AddAction(actionCancel);

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alertController, true, null);
        }

        public override void RunJavaScriptTextInputPanel(WKWebView webView, string prompt, string defaultText, WKFrameInfo frame, Action<string> completionHandler)
        {
            var alertController = UIAlertController.Create("", prompt, UIAlertControllerStyle.Alert);

            alertController.AddTextField((UITextField obj) => obj.Text = defaultText);
            var actionOk = UIAlertAction.Create("Ok", UIAlertActionStyle.Default, (UIAlertAction obj) =>
            {
                string text = alertController.TextFields.Any() ? alertController.TextFields.First().Text : defaultText;
                completionHandler(text);
            });
            var actionCancel = UIAlertAction.Create("Cancel", UIAlertActionStyle.Default, (UIAlertAction obj) =>
             {
                 completionHandler(null);
             });

            alertController.AddAction(actionOk);
            alertController.AddAction(actionCancel);

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alertController, true, null);
        }
#endregion
    }
}
