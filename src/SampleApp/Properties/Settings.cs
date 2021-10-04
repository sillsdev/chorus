using Chorus.Properties;

namespace SampleApp.Properties {
	internal sealed partial class Settings
	{
		public Settings()
		{
			SettingsUtils.ValidateProperties(Properties);
		}
	}
}
