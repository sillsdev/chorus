using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chorus.annotations
{
	public class ChorusNotesUser
	{
		public ChorusNotesUser(string name)
		{
			Name = name;
		}

		public string Name { get; set; }
	}
}