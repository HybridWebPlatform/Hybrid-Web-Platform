using System;
namespace HybridWebControl
{
	[AttributeUsage(AttributeTargets.Event)]
	public class JsFunctionCallbackAttribute : Attribute
	{
		public string JsCallbackName
		{
			get;
			private set;
		}

		public JsFunctionCallbackAttribute(string jsCallbackName)
		{
			JsCallbackName = jsCallbackName;
		}
	}
}
