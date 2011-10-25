using System;
using System.Linq;

namespace Chorus.VcsDrivers.Mercurial
{
	public interface IHgTransport : IDisposable
	{
		void Push();
		bool Pull();
	}
}
