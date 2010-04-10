using Chorus.FileTypeHanders;
using Chorus.FileTypeHanders.FieldWorks;
using NUnit.Framework;

namespace LibChorus.Tests.FileHandlers.FieldWorks
{
	/// <summary>
	/// Test the FieldWorksFileHandler implementation of the IChorusFileTypeHandler interface.
	/// </summary>
	[TestFixture]
	public class FieldWorksFileHandlerTests
	{
		private readonly IChorusFileTypeHandler m_handler;

		public FieldWorksFileHandlerTests()
		{
			m_handler = new FieldWorksFileHandler();
		}

		/// <summary>
		/// Make sure the CanDiffFile method is not implemented.
		/// </summary>
		[Test]
		[ExpectedException("NotImplementedException")]
		public void CanDiffFile_NotImplemented()
		{
			m_handler.CanDiffFile("bogusPathname");
		}

		/// <summary>
		/// Make sure the CanMergeFile method is not implemented.
		/// </summary>
		[Test]
		[ExpectedException("NotImplementedException")]
		public void CanMergeFile_NotImplemented()
		{
			m_handler.CanMergeFile("bogusPathname");
		}

		/// <summary>
		/// Make sure the CanPresentFile method is not implemented.
		/// </summary>
		[Test]
		[ExpectedException("NotImplementedException")]
		public void CanPresentFile_NotImplemented()
		{
			m_handler.CanPresentFile("bogusPathname");
		}

		/// <summary>
		/// Make sure the CanValidateFile method is not implemented.
		/// </summary>
		[Test]
		[ExpectedException("NotImplementedException")]
		public void CanValidateFile_NotImplemented()
		{
			m_handler.CanValidateFile("bogusPathname");
		}

		/// <summary>
		/// Make sure the Do3WayMerge method is not implemented.
		/// </summary>
		[Test]
		[ExpectedException("NotImplementedException")]
		public void Do3WayMerge_NotImplemented()
		{
			m_handler.Do3WayMerge(null);
		}

		/// <summary>
		/// Make sure the GetChangePresenter method is not implemented.
		/// </summary>
		[Test]
		[ExpectedException("NotImplementedException")]
		public void GetChangePresenter_NotImplemented()
		{
			m_handler.GetChangePresenter(null, null);
		}

		/// <summary>
		/// Make sure the ValidateFile method is not implemented.
		/// </summary>
		[Test]
		[ExpectedException("NotImplementedException")]
		public void ValidateFile_NotImplemented()
		{
			m_handler.ValidateFile("bogusPathname", null);
		}

		/// <summary>
		/// Make sure the DescribeInitialContents method is not implemented.
		/// </summary>
		[Test]
		[ExpectedException("NotImplementedException")]
		public void DescribeInitialContents_NotImplemented()
		{
			m_handler.DescribeInitialContents(null, null);
		}

		/// <summary>
		/// Make sure the GetExtensionsOfKnownTextFileTypes method is not implemented.
		/// </summary>
		[Test]
		[ExpectedException("NotImplementedException")]
		public void GetExtensionsOfKnownTextFileTypes_NotImplemented()
		{
			m_handler.GetExtensionsOfKnownTextFileTypes();
		}
	}
}
