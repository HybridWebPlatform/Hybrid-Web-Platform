using Xamarin.Forms;

namespace XamlExample
{
	public partial class XamlExamplePage : ContentPage
	{
		public XamlExamplePage()
		{
			InitializeComponent();
		}


		protected override void OnAppearing()
		{
			base.OnAppearing();
			WebView.LoadPage(new System.Uri("http://google.com"));
		}
	}
}
