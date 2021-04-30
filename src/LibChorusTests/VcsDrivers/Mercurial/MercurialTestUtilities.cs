using System;
using System.Collections.Generic;
using System.IO;
using Chorus.VcsDrivers.Mercurial;
using Nini.Ini;
using SIL.IO;
using SIL.PlatformUtilities;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	public class MercurialIniForTests : IDisposable
	{
		private readonly string _mercurialIniFilePath;
		private readonly string _mercurialIniBackupFilePath;

		public MercurialIniForTests()
		{
			_mercurialIniFilePath = Path.Combine(Chorus.MercurialLocation.PathToMercurialFolder, "mercurial.ini");
			_mercurialIniBackupFilePath = _mercurialIniFilePath + ".bak";
			File.Copy(_mercurialIniFilePath, _mercurialIniBackupFilePath, true);
			UpdateExtensions();
		}

		public void Dispose()
		{
			File.Copy(_mercurialIniBackupFilePath, _mercurialIniFilePath, true);
			File.Delete(_mercurialIniBackupFilePath);
		}

		private static void UpdateExtensions()
		{
			var extensions = new Dictionary<string, string>();
			if (!Platform.IsMono)
				extensions.Add("eol", ""); //for converting line endings on windows machines
			extensions.Add("hgext.graphlog", ""); //for more easily readable diagnostic logs
			extensions.Add("convert", ""); //for catastrophic repair in case of repo corruption
			string fixUtfFolder = FileLocationUtilities.GetDirectoryDistributedWithApplication(false, "MercurialExtensions", "fixutf8");
			if (!string.IsNullOrEmpty(fixUtfFolder))
				extensions.Add("fixutf8", Path.Combine(fixUtfFolder, "fixutf8.py"));

			var doc = HgRepository.GetMercurialConfigInMercurialFolder();
			SetExtensions(doc, extensions);
			doc.SaveAndThrowIfCannot();
		}

		private static void SetExtensions(IniDocument doc, IEnumerable<KeyValuePair<string, string>> extensionDeclarations)
		{
			var section = doc.Sections.GetOrCreate("extensions");
			foreach (var pair in extensionDeclarations)
			{
				section.Set(pair.Key, pair.Value);
			}
		}

	}

	public class MercurialIniHider : IDisposable
	{
		private readonly string _mercurialIniFilePath;
		private readonly string _mercurialIniBackupFilePath;

		public MercurialIniHider()
		{
			_mercurialIniFilePath = Path.Combine(Chorus.MercurialLocation.PathToMercurialFolder, "mercurial.ini");
			_mercurialIniBackupFilePath = _mercurialIniFilePath + ".bak";
			File.Copy(_mercurialIniFilePath, _mercurialIniBackupFilePath, true);
		}

		public void Dispose()
		{
			File.Copy(_mercurialIniBackupFilePath, _mercurialIniFilePath, true);
			File.Delete(_mercurialIniBackupFilePath);
		}
	}
}
