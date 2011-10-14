using System;
using Palaso.Progress.LogBox;

namespace Chorus.VcsDrivers.Mercurial
{
	public class HgNormalTransport : IHgTransport
	{
		private HgRepository _repo;
		private string _targetUri;
		private string _targetLabel;
		private IProgress _progress;

		public HgNormalTransport(HgRepository repo, string targetLabel, string targetUri, IProgress progress)
		{
			_repo = repo;
			_targetUri = targetUri;
			_targetLabel = targetLabel;
			_progress = progress;
		}

		/* CJH 2011-09-19
		 * In the process of separating out the Push and Pull functionality from HgRepository, I realized that too many private methods needed to become public
		 * in order to fully extract the Push and Pull methods.  This is likely because there are methods on HgRepository that have little to do with the
		 * model and more to do with the HgRunner class and executing Hg commands.  A future refactoring might include moving functionality from HgRepository over
		 * to HgRunner, for example methods like Execute() and SurroundWithQuotes()
		 *
		 * The scaffolding is set to move the Push and Pull functionality out of HgRepository and into here.  For now though we just call the appropriate
		 * "normal service" methods on HgRepository. The "resumable service" methods are implemented outside of HgRepository in its own transport class.
		 */

		public void Push()
		{
			_repo.PushToTarget(_targetLabel, _targetUri);
		}

		public bool Pull()
		{
			return _repo.PullFromTarget(_targetLabel, _targetUri);
		}

		public void Dispose()
		{
			// how do we clean up here?  Do we need to do anything?
		}
	}
}