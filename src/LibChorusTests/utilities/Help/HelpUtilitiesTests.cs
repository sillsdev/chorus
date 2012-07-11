using System.Reflection;
using Chorus.Utilities.Help;
using NUnit.Framework;

namespace LibChorus.Tests.utilities.Help
{
	[TestFixture]
	public class HelpUtilitiesTests
	{
		[Test]
		public void Stream_to_Helpfile_Exists()
		{
			Assert.IsNotNull(Assembly.GetAssembly(typeof (HelpUtils)).GetManifestResourceStream("Chorus.Send_Receive_Help.chm"));
		}
	}
}