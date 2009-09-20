using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.UI.Misc
{
	public partial class ReadinessDialog : Form
	{
		public ReadinessDialog()
		{
			InitializeComponent();
		}

		public static bool ChorusIsReady
		{
			get { return string.IsNullOrEmpty(HgRepository.GetEnvironmentReadinessMessage("en")); }
		}
	}
}
