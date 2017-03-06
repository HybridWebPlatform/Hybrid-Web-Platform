using System;
namespace HybridWebControl
{
	[AttributeUsage(AttributeTargets.Field)]
	public class JsFunctionInjectAttribute : Attribute
	{
		public string JsFunctionName
		{
			get;
			private set;
		}

		public string JsFunctionBody
		{
			get;
			private set;
		}

		public JsFunctionInjectAttribute(string jsFunctionName, string jsFunctionBody)
		{
			JsFunctionName = jsFunctionName;
			JsFunctionBody = jsFunctionBody;
		}
	}
}
