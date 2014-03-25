using System.ServiceProcess;

namespace ChorusHub
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main()
		{
			var servicesToRun = new ServiceBase[] 
			{ 
				new ChorusHub() 
			};
			ServiceBase.Run(servicesToRun);
		}
	}
}
