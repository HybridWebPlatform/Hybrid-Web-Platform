using System;
using System.Runtime.Serialization;

namespace HybridWebPlatform.HybridWeb.Contracts
{
	[DataContract]
	public class HybridJavascriptMessage
	{
		[DataMember(Name = "a")]
		public string Action { get; set; }
		[DataMember(Name = "d")]
		public object Data { get; set; }
		[DataMember(Name = "c")]
		public string Callback { get; set; }
	}
}
