using System;
using System.Collections.Generic;
using System.Text;

namespace Baton
{
	public class BootStrapper
	{
		public Shell CreateShell()
		{
			var builder = new Autofac.Builder.ContainerBuilder();
			builder.Register<Shell>();
			var container = builder.Build();
			return container.Resolve<Shell>();
		}
	}
}
