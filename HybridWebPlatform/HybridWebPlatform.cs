using System;

using Xamarin.Forms;
using System.Threading.Tasks;
using HybridWebPlatform.HybridWeb.Contracts;

namespace HybridWebPlatform
{
	public class App : Application
	{
		HybridWeb.HybridWebView vw;

		public App()
		{
			vw = new HybridWeb.HybridWebView();
			// The root page of your application
			var content = new ContentPage
			{
				Title = "HybridWebPlatform",
				Content = vw
			};

			MainPage = new NavigationPage(content);
		}

		protected override void OnStart()
		{
			vw.LoadPage(new Uri("http://google.com"));
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
