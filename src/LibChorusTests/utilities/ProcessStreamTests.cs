﻿// // Copyright (c) 2024-2024 SIL International
// // This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Diagnostics;
using Chorus.Utilities;
using NUnit.Framework;
using SIL.PlatformUtilities;

namespace LibChorus.Tests.utilities
{
	[TestFixture]
	public class ProcessStreamTests
	{
		[Test]
		public void OutputsStdOut()
		{
			var expectedOutput = "Hello, World!";
			var ps = new ProcessStream();
			var process = new Process();
			if (Platform.IsUnix) {
				process.StartInfo.FileName = "sh";
				process.StartInfo.Arguments = $"-c \"echo '{expectedOutput}'\"";
			} else {
				process.StartInfo.FileName = "cmd.exe";
				process.StartInfo.Arguments = "/c echo " + expectedOutput;
			}
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.Start();
			var readReturnCode = ps.Read(ref process, 10);
			Assert.AreEqual(1, readReturnCode);
			Assert.AreEqual(expectedOutput, ps.StandardOutput.Trim());
			process.WaitForExit();
		}

		[Test]
		public void OutputsStdErr()
		{
			var expectedOutput = "Hello, World!";
			var ps = new ProcessStream();
			var process = new Process();
			if (Platform.IsUnix) {
				process.StartInfo.FileName = "sh";
				process.StartInfo.Arguments = $"-c \"echo '{expectedOutput}' 1>&2\"";
			} else {
				process.StartInfo.FileName = "cmd.exe";
				process.StartInfo.Arguments = "/c echo " + expectedOutput + " 1>&2";
			}
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardError = true;
			process.Start();
			var readReturnCode = ps.Read(ref process, 10);
			Assert.AreEqual(1, readReturnCode);
			Assert.AreEqual(expectedOutput, ps.StandardError.Trim());
			process.WaitForExit();
		}

		[Test]
		public void TimesOut()
		{
			var ps = new ProcessStream();
			var process = new Process();
			if (Platform.IsUnix) {
				process.StartInfo.FileName = "sh";
				process.StartInfo.Arguments = "-c \"sleep 10s && echo -n test\"";
			} else {
				process.StartInfo.FileName = "cmd.exe";
				process.StartInfo.Arguments = "/c waitfor /T 10 pause1 & echo test";
			}
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.Start();
			var readReturnCode = ps.Read(ref process, 1);
			Assert.AreEqual(string.Empty, ps.StandardOutput);
			Assert.AreEqual(string.Empty, ps.StandardError);
			Assert.AreEqual(ProcessStream.kTimedOut, readReturnCode);
			process.Kill();
		}

		[Test]
		public void DoesNotTimeoutIfFinishedInTime()
		{
			var ps = new ProcessStream();
			var process = new Process();
			if (Platform.IsUnix) {
				process.StartInfo.FileName = "sh";
				process.StartInfo.Arguments = "-c \"sleep 1s && echo -n test\"";
			} else {
				process.StartInfo.FileName = "cmd.exe";
				process.StartInfo.Arguments = "/c waitfor /T 1 pause2 & echo test";
			}
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.Start();
			var readReturnCode = ps.Read(ref process, 10);
			Assert.That(ps.StandardOutput, Does.Contain("test"));
			if (Platform.IsUnix) {
				Assert.That(ps.StandardError?.TrimEnd(), Is.Null.Or.Empty); //should be empty because sleep does not give an error
			} else {
				Assert.That(ps.StandardError, Is.Not.Null.Or.Empty); //should not be empty because waitfor gives an error
			}
			Assert.AreEqual(1, readReturnCode);
			process.WaitForExit();
		}
	}
}