using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using Chorus.UI.Misc;
using Chorus.UI.Notes.Browser;
using Chorus.UI.Review;
using Chorus.UI.Settings;
using Chorus.UI.Sync;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus
{
	public class BootStrapper :IDisposable
	{
		private readonly string _projectPath;
		private IContainer _container;

		public BootStrapper(string projectPath)
		{
			_projectPath = projectPath;
		}

		public Shell CreateShell(BrowseForRepositoryEvent browseForRepositoryEvent)
		{
			var builder = new Autofac.Builder.ContainerBuilder();

			ChorusUIComponentsInjector.Inject(builder, _projectPath, SyncUIFeatures.Advanced);

			builder.Register<BrowseForRepositoryEvent>(browseForRepositoryEvent).SingletonScoped();

			builder.Register<IChorusUser>(c => new ChorusUser(c.Resolve<HgRepository>().GetUserIdInUse()));

			builder.Register<Shell>();

			_container = builder.Build();
			var shell= _container.Resolve<Shell>();

			shell.AddPage("Review", _container.Resolve<HistoryPage>());
			shell.AddPage("Notes", _container.Resolve<NotesBrowserPage>());
			shell.AddPage("Send/Receive", _container.Resolve<SyncPanel>());
			shell.AddPage("Settings", _container.Resolve<SettingsView>());
			shell.AddPage("Troubleshooting", _container.Resolve<TroubleshootingView>());

			return shell;
		}



		public void Dispose()
		{
			_container.Dispose();
		}
	}
}