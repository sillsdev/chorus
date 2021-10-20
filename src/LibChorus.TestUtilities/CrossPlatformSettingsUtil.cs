using System.Configuration;
using NUnit.Framework;
using SIL.Settings;

namespace LibChorus.TestUtilities
{
	public static class CrossPlatformSettingsUtil
	{

		/// <summary>
		/// Verifies that each property has its provider set to <see cref="CrossPlatformSettingsProvider"/>
		/// </summary>
		public static void ValidateProperties(SettingsPropertyCollection properties)
		{
			foreach (SettingsProperty property in properties)
			{
				Assert.That(property.Provider, Is.AssignableTo<CrossPlatformSettingsProvider>(),
					$"Property '{property.Name}' needs the Provider string set to {typeof(CrossPlatformSettingsProvider)}");
			}
		}
	}
}
