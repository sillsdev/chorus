using System.Configuration;
using System.Diagnostics;
using SIL.Settings;

namespace SampleApp.Properties {
	internal sealed partial class Settings
	{
		public Settings()
		{
			foreach (SettingsProperty property in Properties)
			{
				Trace.Assert(property.Provider is CrossPlatformSettingsProvider,
					$"Property '{property.Name}' needs the Provider string set to {typeof(CrossPlatformSettingsProvider)}");
			}
		}
	}
}
