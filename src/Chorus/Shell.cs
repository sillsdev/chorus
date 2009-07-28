using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;

namespace Baton
{
	public partial class Shell : Form
	{
		private readonly BrowseForRepositoryEvent _browseForRepositoryEvent;

		public Shell(HgRepository repository, BrowseForRepositoryEvent browseForRepositoryEvent)
		{

			_browseForRepositoryEvent = browseForRepositoryEvent;
			InitializeComponent();
			Text = Application.ProductName + " "+Application.ProductVersion +" - "+ repository.PathToRepo;
			_tabControl.TabPages.Clear();
		 }

		public void AddPage(string label,Control form)
		{
			var page = new TabPage(label);
			form.Dock = DockStyle.Fill;
			page.Controls.Add(form);
			_tabControl.TabPages.Add(page);
		}

		private void OpenRepositoryButton_Click(object sender, System.EventArgs e)
		{
			_browseForRepositoryEvent.Raise(null);
		}
	}
}