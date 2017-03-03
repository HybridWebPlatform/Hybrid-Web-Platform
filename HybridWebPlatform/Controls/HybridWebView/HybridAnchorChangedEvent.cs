using System;
namespace HybridWebPlatform.HybridWeb.Contracts
{
	public struct HybridAnchorChangedEvent
	{
		public string OldHashValue { get; set; }
		public string NewHashValue { get; set; }
	}
}
