using SIL.Settings;

namespace Chorus.Properties {
	internal sealed partial class Settings
	{
		public Settings()
		{
			CrossPlatformSettingsProvider.ValidateProperties(Properties);
		}
	}
}
