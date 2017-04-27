using System;
using HybridWebPlatform;
using Xamarin.Forms;


namespace CodeExample
{
	public class App : Application
	{
		HybridWebPlatformView webView;

		public App()
		{
			webView = new HybridWebPlatformView();
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
}
