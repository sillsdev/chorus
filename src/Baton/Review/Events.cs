using System;
using System.Collections.Generic;
using System.Text;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;

namespace Baton.Review
{
	public class Event<TPayload>
	{
		private readonly List<Action<TPayload>> _subscribers = new List<Action<TPayload>>();

		public void Subscribe(Action<TPayload> action)
		{
			if (!_subscribers.Contains(action))
			{
				_subscribers.Add(action);
			}
		}
		public void Raise(TPayload descriptor)
		{
			foreach (Action<TPayload> subscriber in _subscribers)
			{
				((Action<TPayload>)subscriber)(descriptor);
			}
		}
	}

	public class RevisionSelectedEvent : Event<RevisionDescriptor>
	{ }

	public class ChangedRecordSelectedEvent : Event<IChangeReport>
	{ }
}
