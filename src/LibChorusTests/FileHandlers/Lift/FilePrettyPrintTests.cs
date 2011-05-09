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
		id='pos1' />
	<entry
		id='pos2' />
	<entry
		id='pos3' />
</lift>";
			const string child =
@"<?xml version='1.0' encoding='utf-8'?><lift version='0.13' producer='SIL.FLEx 7.0.0.40590'><entry id='pos3'/><entry id='pos1'/><entry id='pos2'/></lift>";

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
		id='pos1' />
	<entry
		id='pos2' />
	<entry
		id='pos3' />
</lift>";
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<lift
	version='0.13'
	producer='SIL.FLEx 7.0.0.40590'><header><stuff></stuff></header>
	<entry
		id='pos1' />
	<entry
		id='pos2' />
	<entry
		id='pos3' />
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
		<stuff></stuff>
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
		id='pos1' />
	<entry
		id='pos2' />
	<entry
		id='pos3' />
</lift>";
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<lift
	version='0.13'
	producer='SIL.FLEx 7.0.0.40590'><header><newstuff></newstuff></header>
	<entry
		id='pos1' />
	<entry
		id='pos2' />
	<entry
		id='pos3' />
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
		<newstuff></newstuff>
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
		id='pos1' />
	<entry
		id='pos2' />
	<entry
		id='pos3' />
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
		id='pos1' />
	<entry
		id='POS2' />
	<entry
		id='pos3' />
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
		id='pos1' />
	<entry
		id='POS2' />
	<entry
		id='pos3' />
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
		id='pos1' />
	<entry
		id='pos2' />
	<entry
		id='pos3' />
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
		id='pos1' />
	<entry
		id='pos2' />
	<entry
		id='pos3' />
</lift>";
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<lift
	version='0.13'
	producer='SIL.FLEx 7.0.0.40590'>
	<entry
		id='pos1' />
	<entry
		id='pos2' />
	<entry
		id='pos3' />
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
	}
}
