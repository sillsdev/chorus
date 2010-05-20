using System;
using System.Windows.Forms;

namespace Chorus.UI.Misc
{
	public partial class ProxySettingsView : UserControl
	{
		private readonly ProxySettingsModel _model;

		public ProxySettingsView(ProxySettingsModel model)
		{
			_model = model;
			InitializeComponent();
		}

		private void ProxySettingsView_Load(object sender, EventArgs e)
		{
			_host.Text = _model.Proxy.Host;
			_port.Text = _model.Proxy.Port;
			_userName.Text = _model.Proxy.UserName;
			_password.Text = _model.Proxy.Password;
			_bypassList.Text = _model.Proxy.BypassList;
		}

		private void ProxySettingsView_Leave(object sender, EventArgs e)
		{
			_model.Proxy.Host = _host.Text;
			_model.Proxy.Port = _port.Text;
			_model.Proxy.UserName = _userName.Text;
			_model.Proxy.Password = _password.Text;
			_model.Proxy.BypassList = _bypassList.Text;
			_model.Save();
		}
	}
}
