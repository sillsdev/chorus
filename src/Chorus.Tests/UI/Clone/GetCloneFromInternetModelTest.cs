using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chorus.UI.Clone;
using Chorus.Utilities;
using NUnit.Framework;

namespace Chorus.Tests.UI.Clone
{
	[TestFixture]
	public class GetCloneFromInternetModelTest
	{
		[Test]
		public void InitFromUri_GivenCompleteUri_AllPropertiesCorrect()
		{
			using (var testFolder = new TempFolder("clonetest"))
			{
				var model = new GetCloneFromInternetModel(testFolder.Path);
				model.InitFromUri("http://john:myPassword@hg-languagedepot.org/tpi?localFolder=tokPisin");
				Assert.AreEqual("tokPisin", model.LocalFolderName);
				Assert.IsTrue(model.ReadyToDownload);
				Assert.AreEqual("http://john:myPassword@hg-languagedepot.org/tpi",model.URL);
			}
		}

		[Test]
		public void URL_AfterConstruction_GoodDefault()
		{
			using (var testFolder = new TempFolder("clonetest"))
			{
				var model = new GetCloneFromInternetModel(testFolder.Path);
				model.AccountName = "account";
				model.Password = "password";
				model.ProjectId = "id";
				Assert.AreEqual("http://account:password@hg-public.languagedepot.org/id", model.URL.ToLower());
			}
		}
	}
}
