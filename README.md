## Abstract
Full-fledged WebView as Xamarin.Forms plugin (XLabs based) with cross-platform C# to JavaScript and JavaScript to C# calls support. Eventually invented for painless hybrid apps creation.

HybridWebView based on great XLabs core components, but it's _not_ based on [XLabs.HybridWebView](https://github.com/XLabs/Xamarin-Forms-Labs/wiki/HybridWebView). Moreover, it's replacing XLabs.HybridWebView as more advanced and not legacy plugin.

## HybridWebView Advantages
It solve the following problems for:

- Xamrin.Forms WebView
  - Allow to call C# from JavaScript
- XLabs.WebView
  - Fix perfromance issue on Android (Hardware accelaration)

## Road Map

- [ ] Enable Hardware accelaration by default (actual for Android)
- [ ] Support UWP
- [ ] Fix keyboard overlap issue on Android
