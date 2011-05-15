using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.Progress.LogBox;

namespace Chorus.VcsDrivers
{
	/// <summary>
	/// common interface for all DVCS repositories.
	/// </summary>
	public interface IDVCSRepository
	{
		/// <summary>
		/// Initialize a newly created repository.
		/// </summary>
		void Init(string newRepositoryPath, IProgress progress);
	}
}
