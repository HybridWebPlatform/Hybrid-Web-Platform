using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;

namespace CodeExample.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			new HybridWebPlatform.iOS.HybridWebPlatformViewRenderer();
			global::Xamarin.Forms.Forms.Init();

			LoadApplication(new App());

			return base.FinishedLaunching(app, options);
		}
	}
}
