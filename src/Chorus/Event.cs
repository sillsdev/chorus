using System;
using System.Collections.Generic;

namespace Chorus
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
		public bool HasSubscribers
		{
			get { return _subscribers.Count > 0; }
		}
	}

	public class Event<TPayload1, TPayload2>
	{
		private readonly List<Action<TPayload1, TPayload2>> _subscribers = new List<Action<TPayload1, TPayload2>>();

		public void Subscribe(Action<TPayload1, TPayload2> action)
		{
			if (!_subscribers.Contains(action))
			{
				_subscribers.Add(action);
			}
		}
		public void Raise(TPayload1 first, TPayload2 second)
		{
			foreach (Action<TPayload1, TPayload2> subscriber in _subscribers)
			{
				((Action<TPayload1, TPayload2>)subscriber)(first, second);
			}
		}
	}


	public class BrowseForRepositoryEvent : Event<string>
	{ }

	public class NotesUpdatedEvent: Event<object>//really, null
	{}

}