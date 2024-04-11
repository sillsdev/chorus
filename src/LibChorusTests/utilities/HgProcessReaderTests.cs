// // Copyright (c) 2024-2024 SIL International
// // This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Diagnostics;
using Chorus.Utilities;
using NUnit.Framework;
using SIL.Progress;

namespace LibChorus.Tests.utilities
{
	[TestFixture]
	public class HgProcessReaderTests
	{
		private static readonly IProgress _progress = new NullProgress();

		private Process Process()
		{
			return new Process()
			{
				StartInfo =
				{
					UseShellExecute = false,
					RedirectStandardInput = true,
					RedirectStandardError = true,
					RedirectStandardOutput = true
				}
			};
		}

		[Test]
		public void OutputsStdOut()
		{
			var expectedOutput = "Hello, World!";
			var ps = new HgProcessOutputReader("/");
			var process = Process();
			process.StartInfo.FileName = "cmd.exe";
			process.StartInfo.Arguments = "/c echo " + expectedOutput;
			process.Start();
			var finished = ps.Read(ref process, 10, _progress);
			Assert.True(finished);
			Assert.AreEqual(expectedOutput, ps.StandardOutput.Trim());
			process.WaitForExit();
		}

		[Test]
		public void OutputsStdErr()
		{
			var expectedOutput = "Hello, World!";
			var ps = new HgProcessOutputReader("/");
			var process = Process();
			process.StartInfo.FileName = "cmd.exe";
			process.StartInfo.Arguments = "/c echo " + expectedOutput + " 1>&2";
			process.Start();
			var finished = ps.Read(ref process, 10, _progress);
			Assert.True(finished);
			Assert.AreEqual(expectedOutput, ps.StandardError.Trim());
			process.WaitForExit();
		}

		[Test]
		public void TimesOut()
		{
			var ps = new HgProcessOutputReader("/");
			var process = Process();
			process.StartInfo.FileName = "cmd.exe";
			process.StartInfo.Arguments = "/c waitfor /T 10 pause3 & echo test";
			process.Start();
			var finished = ps.Read(ref process, 1, _progress);
			Assert.AreEqual(string.Empty, ps.StandardOutput);
			Assert.AreEqual(string.Empty, ps.StandardError);
			Assert.False(finished);
			process.Kill();
		}

		[Test]
		public void DoesNotTimeoutIfFinishedInTime()
		{
			var ps = new HgProcessOutputReader("/");
			var process = Process();
			process.StartInfo.FileName = "cmd.exe";
			process.StartInfo.Arguments = "/c waitfor /T 1 pause4 & echo test";
			process.Start();
			var finished = ps.Read(ref process, 10, _progress);
			Assert.True(finished);
			Assert.That(ps.StandardError, Is.Not.Null.Or.Empty); //should not be empty because waitfor gives an error
			Assert.That(ps.StandardOutput, Is.Not.Null.And.Contains("test"));
			process.WaitForExit();
		}

		[Test]
		public void DoesNotTimeoutWithIntermediateOutput()
		{
			int segmentTime = 2;
			int totalSeconds = 2 * 3;

			var ps = new HgProcessOutputReader("/");
			var process = Process();
			process.StartInfo.FileName = "cmd.exe";
			//wait then output repeat, since there's output the whole time our read should not timeout
			process.StartInfo.Arguments = string.Format("/c waitfor /T {0} pause5 & echo test & waitfor /T {0} pause6 & echo test2 & waitfor /T {0} pause7 & echo test3", segmentTime);
			process.Start();
			//even though it's waiting for less than the full time there's still progress made so it should finish
			var finished = ps.Read(ref process, totalSeconds - segmentTime, _progress);
			Assert.True(finished);
			Assert.That(ps.StandardError, Is.Not.Null.Or.Empty); //should not be empty because waitfor gives an error
			Assert.That(ps.StandardOutput, Is.Not.Null.And.Contains("test").And.Contains("test2").And.Contains("test3"));
			process.WaitForExit();
		}
	}
}