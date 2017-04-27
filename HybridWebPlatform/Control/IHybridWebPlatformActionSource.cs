using System;
using System.Threading.Tasks;

namespace HybridWebPlatform
{
	public interface IHybridWebPlatformActionSource
	{
		bool CanGoBack
		{
			get;
		}

		bool CanGoForward
		{
			get;
		}

		bool IsLoading
		{
			get;
		}

		string CurrentUrl
		{
			get;
		}

		event Func<Uri, bool> PageLoadRequest;
		event Action<Uri> PageLoadStarted;
		event Action<Uri> PageLoadFinished;
		event Action<Uri, string, int> PageLoadError;
		event Action<string> JavascriptExecuted;
		event Action<Uri> PageLoadInNewWindowRequest;

		void GoBack();
		void GoForward();
		void RefreshPage();
		void LoadPage(Uri page);
		void ExecuteJavascript(string javascript);
	}
}
