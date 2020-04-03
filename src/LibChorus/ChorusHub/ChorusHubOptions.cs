using System;
using System.IO;
using Chorus.Utilities.code;

namespace Chorus.ChorusHub
{
	public static class ChorusHubOptions
	{
		private static string _rootDirectory = LinuxUtils.IsUnix ? Path.Combine(Environment.GetEnvironmentVariable("HOME"), "ChorusHub") : @"C:\ChorusHub";

		//these numbers were selected by looking at the IANA registry and intentionally *not* picking,
		//"undefined" ones (which could become defined in the future), but rather ones already assigned to stuff
		//that looks unlikely to be running on the same subnet
		/// <summary>
		/// "Controller Pilot Data Link Communication"
		/// </summary>
		public const int AdvertisingPort = 5911;
		/// <summary>
		/// "Flight Information Services"
		/// </summary>
		public const int ServicePort = 5912;
		/// <summary>
		/// "Automatic Dependent Surveillance"
		/// </summary>
		public const int MercurialPort = 5913;

		/// <summary>
		/// Path to a folder where all the repositories will be placed.
		/// </summary>
		public static string RootDirectory
		{
			get
			{
				if (!Directory.Exists(_rootDirectory))
					Directory.CreateDirectory(_rootDirectory);

				return _rootDirectory;
			}
			internal set
			{
				// Only to be used by tests.
				_rootDirectory = value;
				if (!Directory.Exists(_rootDirectory))
					Directory.CreateDirectory(_rootDirectory);
			}
		}

		//Note: Before we can make all the ports user-definable, we have to think about the client; how does the
		//the client know which port to listen on? Once it hears from the Advertiser, the Advertiser could tell
		//it the other ports. But does that mean that at least the Advertiser must be on a fixed port?

/*        [Argument(ArgumentTypes.AtMostOnce,
			HelpText =
				"Port used to advertise the availability of this Chorus Hub to other computers using Chorus Send/Receive"
			, LongName = "advertisingPort", DefaultValue = AdvertisingPort, ShortName = "ap")] public int
			AdvertisingPort;

		[Argument(ArgumentTypes.AtMostOnce,
			HelpText =
				"Port used to provide Mercurial services to other computers using Chorus Send/Receive"
			, LongName = "mercurialPort", DefaultValue = MercurialPort, ShortName = "mp")] public int MercurialPort;

		[Argument(ArgumentTypes.AtMostOnce,
			HelpText =
				"Port used to provide special Chorus Hub services to other computers using Chorus Send/Receive"
			, LongName = "servicePort", DefaultValue = ServicePort, ShortName = "sp")] public int ServicePort;
 */
	}
}