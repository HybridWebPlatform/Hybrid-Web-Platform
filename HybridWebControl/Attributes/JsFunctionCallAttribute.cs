using System;
namespace HybridWebControl
{
	[AttributeUsage(AttributeTargets.Field)]
	public class JsFunctionCallAttribute : Attribute
	{
		public string JsFunctionName
		{
			get;
			private set;
		}

		public JsFunctionCallAttribute(string jsFunctionName)
		{
			JsFunctionName = jsFunctionName;
		}
	}
}
