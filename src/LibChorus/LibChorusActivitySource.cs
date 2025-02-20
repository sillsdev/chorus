// Copyright (c) 2025-2025 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Diagnostics;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus
{
	public static class LibChorusActivitySource
	{
		public const string ActivitySourceName = "SIL.LibChorus";
		internal static readonly ActivitySource Value = new ActivitySource(ActivitySourceName);

		public static void TagResumableParameters(this Activity activity, string direction, HgResumeApiParameters request)
		{
			activity.SetTag($"app.chorus.resumable.{direction}.chunk-size", request.ChunkSize);
			activity.SetTag($"app.chorus.resumable.{direction}.bundle-size", request.BundleSize);
			activity.SetTag($"app.chorus.resumable.{direction}.start-of-window", request.StartOfWindow);
			activity.SetTag($"app.chorus.resumable.{direction}.trans-id", request.TransId);
			activity.SetTag($"app.chorus.resumable.{direction}.repo-id", request.RepoId);
			activity.SetTag($"app.chorus.resumable.{direction}.quantity", request.Quantity);
		}
	}
}