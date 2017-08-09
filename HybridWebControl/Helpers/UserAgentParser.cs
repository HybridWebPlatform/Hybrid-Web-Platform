using System;
using System.Collections.Generic;
using System.Linq;

namespace HybridWebControl
{
    public static class UserAgentParser
    {
        public static string ConvertToNativeBrowser(this String userAgent)
        {
			var userAgentParams = new List<string>(userAgent.Split(new char[] { ' ' }));

            string version = userAgentParams.FirstOrDefault(param => param.Contains("Version"));
			if (!String.IsNullOrEmpty(version))
				userAgentParams.Remove(version);
            return String.Join(" ", userAgentParams);
        }
    }
}
