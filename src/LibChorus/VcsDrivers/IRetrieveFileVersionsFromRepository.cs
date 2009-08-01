using System;
using System.Collections.Generic;
using System.Text;

namespace Chorus.VcsDrivers
{
	public interface IRetrieveFileVersionsFromRepository
	{
		/// <summary>
		/// Gets a version of a file  from a repository
		/// </summary>
		/// <returns>path to a temp file. caller is responsible for deleting the file.</returns>
		string RetrieveHistoricalVersionOfFile(string relativePath, string versionDescriptor);

		string GetCommonAncestorOfRevisions(string rev1, string rev2);
	}
}