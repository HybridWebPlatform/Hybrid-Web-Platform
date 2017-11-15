using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HybridWebControl.Contracts;
using Xamarin.Forms;
using XLabs.Ioc;
using XLabs.Serialization;

namespace HybridWebControl
{
	public class HybridWebView : View
	{
        public const int NativeAppCookieExpiresInS = 365 * 24 * 60 * 60;

		private string currentHash;
		private string newHash;
		private IHybridWebViewActionSource actionSource;

		private readonly IJsonSerializer jsonSerializer;
		private readonly Dictionary<string, Action<string>> registeredActions;
		private readonly Dictionary<string, Func<string, object[]>> registeredFunctions;

		public HybridWebView() : this((Resolver.IsSet ? Resolver.Resolve<IJsonSerializer>() : null)
				?? DependencyService.Get<IJsonSerializer>() ?? new SystemJsonSerializer())
		{
		}

		public HybridWebView(IJsonSerializer jsonSerializer)
		{
			this.jsonSerializer = jsonSerializer;
			this.registeredActions = new Dictionary<string, Action<string>>();
			this.registeredFunctions = new Dictionary<string, Func<string, object[]>>();
			RegisterHybridInternalJavascriptCallbacks();
		}

        public string FutureLoadedPageCookieHost { get; private set; } = null;
		public string FutureLoadedPageCookieName { get; private set; } = null;
		public string FutureLoadedPageCookieValue { get; private set; } = null;

        public bool IsCookieSetRequested
        {
            get
            {
                return this.FutureLoadedPageCookieName != null;
            }
        }

		public bool CanGoBack
		{
			get
			{
				return actionSource.CanGoBack;
			}
		}

		public bool CanGoForward
		{
			get
			{
				return actionSource.CanGoForward;
			}
		}

		public string CurrentUrl
		{
			get
			{
				return actionSource.CurrentUrl;
			}
		}

		public bool IsLoading
		{
			get
			{
				return actionSource.IsLoading;
			}
		}

		public string CurrentHashAnchor
		{
			get
			{
				return currentHash;
			}
		}

        public string UserAgent 
        {
            get
            {
                return actionSource.UserAgent;
            }
            set
            {
                actionSource.UserAgent = value;
            }
            
        }

        //Required browser actions

        public event Func<Uri, bool> PageLoadRequest;
		public event Action<Uri> PageLoadStarted;
		public event Action<Uri> PageLoadFinished;
		public event Action<Uri, string, int> PageLoadError;
		public event Action<Uri> NewWebBrowserWindowOpenRequest;
        public event Func<Uri, bool> ShouldLoadInNewWindowRequest;
        public event Action DidCloseExternalWebViewWindow;
		//Required Js actions

		public event Action<string> HashAnchorChanged;
		public event Action<string> JavascriptExecutionError;

		public void GoBack()
		{
			actionSource.GoBack();
		}

		public void GoForward()
		{
			actionSource.GoForward();
		}

		public void RefreshPage()
		{
			actionSource.RefreshPage();
		}

		public void RegisterCallback(string name, Action<string> action)
		{
			this.registeredActions.Add(name, action);
		}

		public bool RemoveCallback(string name)
		{
			return this.registeredActions.Remove(name);
		}

		public void RegisterNativeFunction(string name, Func<string, object[]> func)
		{
			this.registeredFunctions.Add(name, func);
		}

		public bool RemoveNativeFunction(string name)
		{
			return this.registeredFunctions.Remove(name);
		}

		public void LoadPage(Uri page)
		{
			string actualLink = page.AbsoluteUri;
			newHash = null;

			if (page.AbsoluteUri.Contains("#"))
			{
				int firstHashPosition = page.AbsoluteUri.IndexOf('#');

				actualLink = page.AbsoluteUri.Substring(0, firstHashPosition);

				newHash = page.AbsoluteUri.Substring(firstHashPosition);
			}

			actionSource.LoadPage(new Uri(actualLink));
		}

		public void LoadHashAnchor(string anchor)
		{
			actionSource.ExecuteJavascript($"window.location.href = '{anchor}';");
		}

		public void ExecuteJavascript(string javascript)
		{
			actionSource.ExecuteJavascript(javascript);
		}

		public void ExecuteJavascriptFunction(string functionName, params object[] parameters)
		{
			var builder = new StringBuilder();

			builder.Append(functionName);
			builder.Append("(");
			for (var n = 0; n < parameters.Length; n++)
			{
				builder.Append(this.jsonSerializer.Serialize(parameters[n]));
				if (n < parameters.Length - 1)
				{
					builder.Append(", ");
				}
			}
			builder.Append(");");

			ExecuteJavascript(builder.ToString());
		}

		public void RemoveAllCallbacks()
		{
			this.registeredActions.Clear();
		}

		public void RemoveAllFunctions()
		{
			this.registeredFunctions.Clear();
		}

		public void SetWebActionSource(IHybridWebViewActionSource source)
		{
			UnregisterOldWebActionSource();
			this.actionSource = source;
			this.actionSource.PageLoadRequest += this.PageLoadRequestHandler;
			this.actionSource.PageLoadStarted += this.PageLoadStartedHandler;
			this.actionSource.PageLoadFinished += this.PageLoadFinishedHandler;
			this.actionSource.PageLoadError += this.PageLoadErrorHandler;
			this.actionSource.PageLoadInNewWindowRequest += this.NewWebBrowserWindowOpenRequestHandler;
			this.actionSource.ShouldLoadInNewWindowRequest += this.ShouldLoadInNewWindowRequestHandler;
			this.actionSource.DidCloseExternalWebViewWindow += this.DidCloseExternalWebViewWindowHandler;
		}

        public void SetCookie(string host, string name, string value)
        {
            FutureLoadedPageCookieHost = host;
            FutureLoadedPageCookieName = name;
            FutureLoadedPageCookieValue = value;
        }

		internal bool TryGetAction(string name, out Action<string> action)
		{
			return this.registeredActions.TryGetValue(name, out action);
		}

		internal bool TryGetFunc(string name, out Func<string, object[]> func)
		{
			return this.registeredFunctions.TryGetValue(name, out func);
		}

		internal void MessageReceived(string message)
		{
			var m = this.jsonSerializer.Deserialize<HybridJavascriptMessage>(message);

			if (m?.Action == null) return;

			Action<string> action;

			if (this.TryGetAction(m.Action, out action))
			{
				action.Invoke(m.Data.ToString());
				return;
			}

			Func<string, object[]> func;

			if (this.TryGetFunc(m.Action, out func))
			{
				Task.Run(() =>
			   {
				   var result = func.Invoke(m.Data.ToString());

				   ExecuteJavascriptFunction($"NativeFuncs[{m.Callback}]", result);
			   });
			}
		}

		internal void HashAnchorChangedCallback(string newHash)
		{
			if (currentHash == newHash) return;
			currentHash = newHash;
			if (HashAnchorChanged != null)
			{
				HashAnchorChanged(newHash);
			}
		}

		internal void OnNewWebBrowserWindowOpenRequest(Uri uri)
		{
			if (NewWebBrowserWindowOpenRequest != null)
			{
				NewWebBrowserWindowOpenRequest(uri);
			}
		}

		internal static string GetInitialJsScript(string nativeFunction)
		{
			var builder = new StringBuilder();
			builder.Append("NativeFuncs = [];");
			builder.Append("function NativeFunc(action, data, callback){");
			builder.Append("  var callbackIdx = NativeFuncs.push(callback) - 1;");
			builder.Append(nativeFunction);
			builder.Append("(JSON.stringify({ a: action, d: data, c: callbackIdx }));}");
			builder.Append(" if (typeof(window.NativeFuncsReady) !== 'undefined') { ");
			builder.Append("   window.NativeFuncsReady(); ");
			builder.Append(" } ");
			builder.Append("window.addEventListener(\"hashchange\", function(){Native('hashChanged',window.location.hash);});");
			return builder.ToString();
		}

		private void RegisterHybridInternalJavascriptCallbacks()
		{
			this.RegisterCallback("hashChanged", HashAnchorChangedCallback);
		}

		private void UnregisterOldWebActionSource()
		{
			if (this.actionSource == null) return;

			this.actionSource.PageLoadRequest -= this.PageLoadRequestHandler;
			this.actionSource.PageLoadStarted -= this.PageLoadStartedHandler;
			this.actionSource.PageLoadFinished -= this.PageLoadFinishedHandler;
			this.actionSource.PageLoadError -= this.PageLoadErrorHandler;
			this.actionSource.PageLoadInNewWindowRequest -= this.NewWebBrowserWindowOpenRequestHandler;
            this.actionSource.ShouldLoadInNewWindowRequest -= this.ShouldLoadInNewWindowRequestHandler;
            this.actionSource.DidCloseExternalWebViewWindow -= this.DidCloseExternalWebViewWindowHandler;
		}

		private bool PageLoadRequestHandler(Uri uri)
		{
			if (PageLoadRequest != null)
			{
				return PageLoadRequest(uri);
			}

			return true;
		}

		private void PageLoadStartedHandler(Uri uri)
		{
			if (PageLoadStarted != null)
			{
				PageLoadStarted(uri);
			}
		}

		private void PageLoadFinishedHandler(Uri uri)
		{
			if (!String.IsNullOrEmpty(newHash))
			{
				LoadHashAnchor(newHash);
				newHash = null;
			}

			if (PageLoadFinished != null)
			{
				PageLoadFinished(uri);
			}
		}

		private void PageLoadErrorHandler(Uri uri, string message, int errorCode)
		{
			if (PageLoadError != null)
			{
				PageLoadError(uri, message, errorCode);
			}
		}

		private void NewWebBrowserWindowOpenRequestHandler(Uri uri)
		{
			if (NewWebBrowserWindowOpenRequest != null)
			{
				NewWebBrowserWindowOpenRequest(uri);
			}
			else
			{
				Device.OpenUri(uri);
			}
		}

        private bool ShouldLoadInNewWindowRequestHandler(Uri uri)
        {
            if (ShouldLoadInNewWindowRequest != null) 
            {
                return ShouldLoadInNewWindowRequest(uri);
            }

            return true;
        }

        private void DidCloseExternalWebViewWindowHandler()
        {
            if (DidCloseExternalWebViewWindow != null)
            {
                DidCloseExternalWebViewWindow();
            }
        }
	}
}
