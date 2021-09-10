using System.Configuration;
using System.Diagnostics;
using SIL.Settings;

namespace Chorus.Properties {
    internal sealed partial class Settings
	{
		public Settings()
		{
			CallPalasoInstead.ValidateProperties(Properties);
		}
	}

	public static class CallPalasoInstead
	{
		/// <summary>
		/// Verifies that each property has its provider set to <see cref="CrossPlatformSettingsProvider"/>
		/// </summary>
		public static void ValidateProperties(SettingsPropertyCollection properties)
		{
			foreach (SettingsProperty property in properties)
			{
				Debug.Assert(property.Provider is CrossPlatformSettingsProvider,
					$"Property '{property.Name}' needs the Provider string set to {typeof(CrossPlatformSettingsProvider)}");
			}
		}

	}
}
