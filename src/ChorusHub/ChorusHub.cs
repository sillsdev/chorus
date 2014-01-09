using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;

namespace ChorusHub
{
	public partial class ChorusHub : ServiceBase
	{
		public ChorusHub()
		{
			InitializeComponent();

			CanPauseAndContinue = false;
		}

		protected override void OnStart(string[] args)
		{
			EventLog.WriteEntry("In OnStart");
		}

		protected override void OnStop()
		{
			EventLog.WriteEntry("In OnStop");
		}
	}
}
