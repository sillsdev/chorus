using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using Chorus.sync;
using Chorus.UI.Misc;
using Chorus.UI.Notes.Browser;
using Chorus.UI.Review;
using Chorus.UI.Settings;
using Chorus.UI.Sync;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress.LogBox;

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

		internal Shell CreateShell(BrowseForRepositoryEvent browseForRepositoryEvent, Arguments arguments)
		{
			var builder = new Autofac.ContainerBuilder();

			ChorusUIComponentsInjector.Inject(builder, _projectPath, SyncUIFeatures.Advanced);

			builder.RegisterInstance(browseForRepositoryEvent).As<BrowseForRepositoryEvent>().SingleInstance();

			//For now, we like the idea of just using the login name.  But
			//this allows someone to override that in the ini (which would be for all users of this machine, then)
			builder.Register<IChorusUser>(c => new ChorusUser(c.Resolve<HgRepository>().GetUserNameFromIni(new NullProgress(), System.Environment.UserName)));

			builder.RegisterType<Shell>();
			if(arguments!=null)
			{
				builder.RegisterInstance(arguments);
				Synchronizer.s_testingDoNotPush = arguments.DontPush; //hack, at this point it would take a lot of plumbing
					//to get this properly to any synchronizer that is created.  Can be fixed if/when we go to the
				//autofac generated factor approach
			}

			_container = builder.Build();
			var shell= _container.Resolve<Shell>();

			var system = new ChorusSystem(_projectPath);

			shell.AddPage("Review", system.WinForms.CreateHistoryPage());
			shell.AddPage("Notes", system.WinForms.CreateNotesBrowser());
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