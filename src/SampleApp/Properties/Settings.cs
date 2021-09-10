using SIL.Settings;

namespace SampleApp.Properties {
	internal sealed partial class Settings
	{
		public Settings()
		{
			CrossPlatformSettingsProvider.ValidateProperties(Properties);
		}
	}
}
