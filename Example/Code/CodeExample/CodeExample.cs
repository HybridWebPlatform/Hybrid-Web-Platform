using System;

using Xamarin.Forms;
using HybridWebControl;

namespace CodeExample
{
	public class App : Application
	{
		HybridWebPlatform<JavascriptFunctionsProvider> webView;

		public App()
		{
			webView = new HybridWebPlatform<JavascriptFunctionsProvider>();
			// The root page of your application
			var content = new ContentPage
			{
				Title = "CodeExample",
				Content = webView
			};

			MainPage = new NavigationPage(content);
		}

		protected override void OnStart()
		{
			webView.LoadPage(new Uri("http://google.com"));
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}
	}

	class JavascriptFunctionsProvider
	{
		[JsFunctionCallback("test1")]
		public event Func<string> DoStuffB;

		[JsFunctionCallAttribute("test2")]
		public Action<string> CallingExistingJsFunction;

		[JsFunctionInjectAttribute("test3", "alert(param1);")]
		public Action<string> InjectJsFunction;
	}
}
