using System;

namespace Chorus.Utilities
{
	public class ShortTermEnvironmentalVariable : IDisposable
	{
		private readonly string _name;
		private string oldValue;

		public ShortTermEnvironmentalVariable(string name, string value)
		{
			_name = name;
			oldValue = Environment.GetEnvironmentVariable(name);
			Environment.SetEnvironmentVariable(name, value);
		}

		///<summary>
		///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		///</summary>
		///<filterpriority>2</filterpriority>
		public void Dispose()
		{
			Environment.SetEnvironmentVariable(_name, oldValue);
		}
	}
}