using System;

namespace Chorus.VcsDrivers
{
	/// <summary>
	/// How Chorus talks to an HTTP repository. Stored per repository in the [chorus] section of .hg/hgrc
	/// (e.g. <c>transport = resumable</c>), independent of the repository's URL.
	/// </summary>
	public enum HgTransportKind
	{
		/// <summary>Plain Mercurial over HTTP (hgweb).</summary>
		HgWeb,
		/// <summary>The chunked, resumable HgResume API, for low-bandwidth or unreliable connections.</summary>
		Resumable
	}

	internal static class HgTransportKindExtensions
	{
		private const string HgWebValue = "hgweb";
		private const string ResumableValue = "resumable";

		public static string ToHgrcValue(this HgTransportKind transport)
		{
			switch (transport)
			{
				case HgTransportKind.HgWeb: return HgWebValue;
				case HgTransportKind.Resumable: return ResumableValue;
				default: throw new ArgumentOutOfRangeException(nameof(transport), transport, null);
			}
		}

		/// <returns>null if the value is missing or not recognized</returns>
		public static HgTransportKind? FromHgrcValue(string value)
		{
			switch (value?.Trim().ToLowerInvariant())
			{
				case HgWebValue: return HgTransportKind.HgWeb;
				case ResumableValue: return HgTransportKind.Resumable;
				default: return null;
			}
		}
	}
}
