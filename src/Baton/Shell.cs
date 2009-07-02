using System.Windows.Forms;

namespace Baton
{
	public partial class Shell : Form
	{
		public Shell()
		{
			InitializeComponent();
			_tabControl.TabPages.Clear();
		 }

		public void AddPage(string label,Control form)
		{
			var page = new TabPage(label);
			form.Dock = DockStyle.Fill;
			page.Controls.Add(form);
			_tabControl.TabPages.Add(page);
		}
	}
}