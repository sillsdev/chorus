// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;

namespace Chorus
{
	/// <summary>
	/// Base implementation for events.
	/// </summary>
	public class Event<TPayload>
	{
		private readonly List<Action<TPayload>> _subscribers = new List<Action<TPayload>>();

		/// <summary>
		/// Subscribe the specified action.
		/// </summary>
		public void Subscribe(Action<TPayload> action)
		{
			if (!_subscribers.Contains(action))
			{
				_subscribers.Add(action);
			}
		}

		/// <summary>
		/// Raise the specified event.
		/// </summary>
		public void Raise(TPayload descriptor)
		{
			foreach (Action<TPayload> subscriber in _subscribers)
			{
				((Action<TPayload>)subscriber)(descriptor);
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance has subscribers.
		/// </summary>
		/// <value><c>true</c> if this instance has subscribers; otherwise, <c>false</c>.</value>
		public bool HasSubscribers
		{
			get { return _subscribers.Count > 0; }
		}
	}

	/// <summary>
	/// Base implementation for events.
	/// </summary>
	public class Event<TPayload1, TPayload2>
	{
		private readonly List<Action<TPayload1, TPayload2>> _subscribers = new List<Action<TPayload1, TPayload2>>();

		/// <summary>
		/// Subscribe the specified event.
		/// </summary>
		public void Subscribe(Action<TPayload1, TPayload2> action)
		{
			if (!_subscribers.Contains(action))
			{
				_subscribers.Add(action);
			}
		}

		/// <summary>
		/// Raise the event
		/// </summary>
		public void Raise(TPayload1 first, TPayload2 second)
		{
			foreach (Action<TPayload1, TPayload2> subscriber in _subscribers)
			{
				((Action<TPayload1, TPayload2>)subscriber)(first, second);
			}
		}
	}

	/// <summary>
	/// BrowseForRepository event.
	/// </summary>
	public class BrowseForRepositoryEvent : Event<string>
	{
	}
}