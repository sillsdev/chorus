using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Chorus.UI
{
	public partial class MainWindow : Form
	{
		[STAThread]
		static void Main(string[] args)
		{
			Application.Run(new MainWindow());
		}

		public MainWindow()
		{
			InitializeComponent();
		}

		private void MainWindow_Load(object sender, EventArgs e)
		{

		}
	}
}