using Chorus.Properties;

namespace ChorusHubApp.Properties {
    internal sealed partial class Settings
	{
		public Settings()
		{
			CallPalasoInstead.ValidateProperties(Properties);
		}
	}
}
