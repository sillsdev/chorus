using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.UI.Misc
{
	public class ProxySettingsModel
	{
		private readonly HgRepository _repository;
		public ProxySpec Proxy { get; set; }
		public ProxySettingsModel(HgRepository repository)
		{
			 _repository = repository;
		}

//        public void Save()
//        {
//            _repository.SetGlobalProxyInfo(Proxy);
//        }
	}

}
