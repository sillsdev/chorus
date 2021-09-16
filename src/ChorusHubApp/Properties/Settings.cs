using SIL.Settings;

namespace ChorusHubApp.Properties {
	internal sealed partial class Settings
	{
		public Settings()
		{
			CrossPlatformSettingsProvider.ValidateProperties(Properties);
		}
	}
}
