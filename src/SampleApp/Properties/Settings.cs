using Chorus.Properties;
using SIL.Settings;

namespace SampleApp.Properties {
    internal sealed partial class Settings
	{
		public Settings()
		{
			CallPalasoInstead.ValidateProperties(Properties);
		}
	}
}
