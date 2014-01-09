using CommandLine;

namespace ChorusHub
{
	public class ChorusHubParameters
	{
		//these numbers were selected by looking at the IANA registry and intentionally *not* picking,
		//"undefined" ones (which could become defined in the future), but rather ones already assigned to stuff
		//that looks unlike to be running on the same subnet
		public const int kAdvertisingPort = 5911;//"Controller Pilot Data Link Communication"
		public const int kServicePort = 5912;//"Flight Information Services"
		public const int kMercurialPort = 5913; //"Automatic Dependent Surveillance"

		[DefaultArgument(ArgumentTypes.AtMostOnce,
			DefaultValue = @"C:\ChorusHub",
			HelpText = "Path to a folder where all the repositories will be placed.")]
		public string RootDirectory;


		//Note: Before we can make all the ports user-definable, we have to think about the client; how does the
		//the client know which port to listen on? Once it hears from the Advertiser, the Adverstiser could tell
		//it the other ports. But does that mean that at least the Advertiser must be on a fixed port?

/*        [Argument(ArgumentTypes.AtMostOnce,
			HelpText =
				"Port used to advertise the availability of this Chorus Hub to other computers using Chorus Send/Receive"
			, LongName = "advertisingPort", DefaultValue = kAdvertisingPort, ShortName = "ap")] public int
			AdvertisingPort;

		[Argument(ArgumentTypes.AtMostOnce,
			HelpText =
				"Port used to provide Mercurial services to other computers using Chorus Send/Receive"
			, LongName = "mercurialPort", DefaultValue = kMercurialPort, ShortName = "mp")] public int MercurialPort;

		[Argument(ArgumentTypes.AtMostOnce,
			HelpText =
				"Port used to provide special Chorus Hub services to other computers using Chorus Send/Receive"
			, LongName = "servicePort", DefaultValue = kServicePort, ShortName = "sp")] public int ServicePort;
 */
	}
}