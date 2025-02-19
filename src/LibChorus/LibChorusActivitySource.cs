// // Copyright (c) 2025-2025 SIL International
// // This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Diagnostics;

namespace Chorus
{
	public static class LibChorusActivitySource
	{
		public const string ActivitySourceName = "SIL.LibChorus";
		internal static readonly ActivitySource Value = new ActivitySource(ActivitySourceName);
	}
}