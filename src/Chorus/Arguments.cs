using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chorus
{
	internal class Arguments
	{
		public Arguments(object[] args)
		{
			DontPush = args.Any(a => (string)(a) == "-noPush");
		}
		public bool DontPush;
	}
}
