using Xamarin.Forms;

namespace XamlExample
{
	public partial class XamlExamplePage : ContentPage
	{
		public XamlExamplePage()
		{
			InitializeComponent();
			WebView.LoadPage(new System.Uri("http://google.com"));
		}
	}
}
