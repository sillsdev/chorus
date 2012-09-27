using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace ChorusHub
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main()
		{
			var hub = new ChorusHubStarter();
			hub.Start();

			try
			{

				while (true)
				{
					hub.CheckForFailedPushes();
					Thread.Sleep(1000);
				}

			}
			catch (Exception e)
			{

				throw;
			}
			finally
			{
				hub.Stop();
			}
		}
	}
}
