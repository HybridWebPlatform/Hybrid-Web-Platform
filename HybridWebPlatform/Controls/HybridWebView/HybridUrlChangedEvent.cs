using System;
namespace HybridWebPlatform.HybridWeb.Contracts
{
	public struct HybridUrlChangedEvent
	{
		public string OldUrlValue { get; set; }
		public string NewUrlValue { get; set; }
	}
}
