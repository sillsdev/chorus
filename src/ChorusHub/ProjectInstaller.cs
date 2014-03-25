using System.ComponentModel;
using System.Configuration.Install;


namespace ChorusHub
{
	[RunInstaller(true)]
	public partial class ProjectInstaller : Installer
	{
		public ProjectInstaller()
		{
			InitializeComponent();
		}
	}
}
