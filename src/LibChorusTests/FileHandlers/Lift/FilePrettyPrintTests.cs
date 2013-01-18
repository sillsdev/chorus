using System.IO;
using Chorus.FileTypeHanders.lift;
using NUnit.Framework;
using Palaso.IO;

namespace LibChorus.Tests.FileHandlers.Lift
{
	[TestFixture]
	public class FilePrettyPrintTests
	{
		[Test]
		public void ShouldMaintainCorrectOrderOfEntries()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<lift
	version='0.13'
	producer='SIL.FLEx 7.0.0.40590'>
	<entry
		id='pos1'
		guid='c1ed46a0-e382-11de-8a39-0800200c9a66' />
	<entry
		id='pos2'
		guid='c1ed46a1-e382-11de-8a39-0800200c9a66' />
	<entry
		id='pos3'
		guid='c1ed46a2-e382-11de-8a39-0800200c9a66' />
</lift>";
			const string child =
@"<?xml version='1.0' encoding='utf-8'?><lift version='0.13' producer='SIL.FLEx 7.0.0.40590'><entry id='pos3' guid='c1ed46a2-e382-11de-8a39-0800200c9a66'/><entry id='pos1' guid='c1ed46a0-e382-11de-8a39-0800200c9a66'/><entry id='pos2' guid='c1ed46a1-e382-11de-8a39-0800200c9a66'/></lift>";

			using (var tempParent = new TempFile(parent))
			using (var tempChild = new TempFile(child))
			{
				LiftFileServices.PrettyPrintFile(tempParent.Path, tempChild.Path);
				var newOutputContents = File.ReadAllText(tempParent.Path);
				Assert.AreEqual(parent.Replace("'", "\""), newOutputContents); // Correct order and format.
			}
		}

		[Test]
		public void NewHeaderShouldBeAdded()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<lift
	version='0.13'
	producer='SIL.FLEx 7.0.0.40590'>
	<entry
		id='pos1'
		guid='c1ed46a3-e382-11de-8a39-0800200c9a66' />
	<entry
		id='pos2'
		guid='c1ed46a4-e382-11de-8a39-0800200c9a66'  />
	<entry
		id='pos3'
		guid='c1ed46a5-e382-11de-8a39-0800200c9a66'  />
</lift>";
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<lift
	version='0.13'
	producer='SIL.FLEx 7.0.0.40590'><header><stuff></stuff></header>
	<entry
		id='pos1'
		guid='c1ed46a3-e382-11de-8a39-0800200c9a66' />
	<entry
		id='pos2'
		guid='c1ed46a4-e382-11de-8a39-0800200c9a66' />
	<entry
		id='pos3'
		guid='c1ed46a5-e382-11de-8a39-0800200c9a66' />
</lift>";

#if MONO
			// Seems that Mono isn't quite the same as windows in the pretty printing.
			const string wellformedHeader =
@"	<header>
		<stuff>
		</stuff>
	</header>";
#else
			const string wellformedHeader =
@"	<header>
		<stuff />
	</header>";
#endif

			using (var tempParent = new TempFile(parent))
			using (var tempChild = new TempFile(child))
			{
				LiftFileServices.PrettyPrintFile(tempParent.Path, tempChild.Path);
				var newOutputContents = File.ReadAllText(tempParent.Path);
				Assert.IsTrue(newOutputContents.Contains(wellformedHeader)); // Header added with good format.
			}
		}

		[Test]
		public void ChangedHeaderShouldBeAdded()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<lift
	version='0.13'
	producer='SIL.FLEx 7.0.0.40590'>
	<header>
		<stuff></stuff>
	</header>
	<entry
		id='pos1'
		guid='c1ed46a6-e382-11de-8a39-0800200c9a66' />
	<entry
		id='pos2'
		guid='c1ed46a7-e382-11de-8a39-0800200c9a66' />
	<entry
		id='pos3'
		guid='c1ed46a8-e382-11de-8a39-0800200c9a66' />
</lift>";
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<lift
	version='0.13'
	producer='SIL.FLEx 7.0.0.40590'><header><newstuff></newstuff></header>
	<entry
		id='pos1'
		guid='c1ed46a6-e382-11de-8a39-0800200c9a66' />
	<entry
		id='pos2'
		guid='c1ed46a7-e382-11de-8a39-0800200c9a66' />
	<entry
		id='pos3'
		guid='c1ed46a8-e382-11de-8a39-0800200c9a66' />
</lift>";

#if MONO
			const string replacedHeader =
@"	<header>
		<newstuff>
		</newstuff>
	</header>";
#else
			const string replacedHeader =
@"	<header>
		<newstuff />
	</header>";
#endif
			using (var tempParent = new TempFile(parent))
			using (var tempChild = new TempFile(child))
			{
				LiftFileServices.PrettyPrintFile(tempParent.Path, tempChild.Path);
				var newOutputContents = File.ReadAllText(tempParent.Path);
				Assert.IsTrue(newOutputContents.Contains(replacedHeader)); // Header changed with good format.
			}
		}

		[Test]
		public void CaseShouldNotMatterInChildIds()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<lift
	version='0.13'
	producer='SIL.FLEx 7.0.0.40590'>
	<header>
		<stuff></stuff>
	</header>
	<entry
		id='pos1'
		guid='c1ed46a9-e382-11de-8a39-0800200c9a66' />
	<entry
		id='pos2'
		guid='c1ed46ab-e382-11de-8a39-0800200c9a66' />
	<entry
		id='pos3'
		guid='c1ed46ac-e382-11de-8a39-0800200c9a66' />
</lift>";
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<lift
	version='0.13'
	producer='SIL.FLEx 7.0.0.40590'>
	<header>
		<stuff></stuff>
	</header>
	<entry
		id='pos1'
		guid='c1ed46a9-e382-11de-8a39-0800200c9a66' />
	<entry
		id='POS2'
		guid='c1ed46ab-e382-11de-8a39-0800200c9a66' />
	<entry
		id='pos3'
		guid='c1ed46ac-e382-11de-8a39-0800200c9a66' />
</lift>";

			using (var tempParent = new TempFile(parent))
			using (var tempChild = new TempFile(child))
			{
				LiftFileServices.PrettyPrintFile(tempParent.Path, tempChild.Path);
				var newOutputContents = File.ReadAllText(tempParent.Path);
				Assert.IsTrue(newOutputContents.Contains("POS2"));
			}
		}

		[Test]
		public void CaseShouldNotMatterInParentIds()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<lift
	version='0.13'
	producer='SIL.FLEx 7.0.0.40590'>
	<header>
		<stuff></stuff>
	</header>
	<entry
		id='pos1'
		guid='c1ed46ad-e382-11de-8a39-0800200c9a66' />
	<entry
		id='POS2'
		guid='c1ed46ae-e382-11de-8a39-0800200c9a66' />
	<entry
		id='pos3'
		guid='c1ed46af-e382-11de-8a39-0800200c9a66' />
</lift>";
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<lift
	version='0.13'
	producer='SIL.FLEx 7.0.0.40590'>
	<header>
		<stuff></stuff>
	</header>
	<entry
		id='pos1'
		guid='c1ed46ad-e382-11de-8a39-0800200c9a66' />
	<entry
		id='pos2'
		guid='c1ed46ae-e382-11de-8a39-0800200c9a66' />
	<entry
		id='pos3'
		guid='c1ed46af-e382-11de-8a39-0800200c9a66' />
</lift>";

			using (var tempParent = new TempFile(parent))
			using (var tempChild = new TempFile(child))
			{
				LiftFileServices.PrettyPrintFile(tempParent.Path, tempChild.Path);
				var newOutputContents = File.ReadAllText(tempParent.Path);
				Assert.IsTrue(newOutputContents.Contains("pos2"));
			}
		}

		[Test]
		public void RemovedHeaderShouldNotBePresent()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<lift
	version='0.13'
	producer='SIL.FLEx 7.0.0.40590'>
	<header>
		<stuff></stuff>
	</header>
	<entry
		id='pos1'
		guid='c1ed46b0-e382-11de-8a39-0800200c9a66' />
	<entry
		id='pos2'
		guid='c1ed46b1-e382-11de-8a39-0800200c9a66' />
	<entry
		id='pos3'
		guid='c1ed46b2-e382-11de-8a39-0800200c9a66' />
</lift>";
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<lift
	version='0.13'
	producer='SIL.FLEx 7.0.0.40590'>
	<entry
		id='pos1'
		guid='c1ed46b0-e382-11de-8a39-0800200c9a66' />
	<entry
		id='pos2'
		guid='c1ed46b1-e382-11de-8a39-0800200c9a66' />
	<entry
		id='pos3'
		guid='c1ed46b2-e382-11de-8a39-0800200c9a66' />
</lift>";

			const string removedHeader =
@"	<header>
		<stuff></stuff>
	</header>";

			using (var tempParent = new TempFile(parent))
			using (var tempChild = new TempFile(child))
			{
				LiftFileServices.PrettyPrintFile(tempParent.Path, tempChild.Path);
				var newOutputContents = File.ReadAllText(tempParent.Path);
				Assert.IsFalse(newOutputContents.Contains(removedHeader)); // Header removed.
			}
		}

		[Test]
		public void EnsureNewEntryWithEntityInIdEndsUpInFile()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<lift
	version='0.13'
	producer='SIL.FLEx 7.0.0.40590'>
	<header>
		<stuff></stuff>
	</header>
	<entry
		guid='c1ecf88e-e382-11de-8a39-0800200c9a66'
		id='FOO &amp;lt; bar' />
	<entry
		guid='c1ecf88f-e382-11de-8a39-0800200c9a66'
		id='pos1' />
	<entry
		guid='c1ecf890-e382-11de-8a39-0800200c9a66'
		id='pos2' />
</lift>";
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<lift
	version='0.13'
	producer='SIL.FLEx 7.0.0.40590'>
	<header>
		<stuff></stuff>
	</header>
	<entry
		guid='c1ecf88f-e382-11de-8a39-0800200c9a66'
		id='pos1' />
	<entry
		guid='c1ecf891-e382-11de-8a39-0800200c9a66'
		id='BAR &amp; lt; foo' />
	<entry
		guid='c1ecf890-e382-11de-8a39-0800200c9a66'
		id='pos2' />
</lift>";

			using (var tempParent = new TempFile(parent))
			using (var tempChild = new TempFile(child))
			{
				LiftFileServices.PrettyPrintFile(tempParent.Path, tempChild.Path);
				var newOutputContents = File.ReadAllText(tempParent.Path);
				Assert.IsFalse(newOutputContents.Contains("FOO &lt; bar"));
				Assert.IsTrue(newOutputContents.Contains("BAR &amp; lt; foo"));
			}
		}
	}
}
