using System;

namespace Chorus.Utilities.code
{
	/// <summary>
	/// Utility class for dealing with Linux specific issues.
	/// </summary>
	static class LinuxUtils
	{
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns <c>true</c> if we're running on Unix, otherwise <c>false</c>.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static bool IsUnix
		{
			get { return Environment.OSVersion.Platform == PlatformID.Unix; }
		}
	}
}
