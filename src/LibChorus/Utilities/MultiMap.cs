using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Chorus.Utilities
{
	/// <summary>
	/// Initially from http://dotnetperls.com/multimap
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public class MultiMap<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>>
	{
		// 1
		Dictionary<TKey, List<TValue>> _dictionary = new Dictionary<TKey, List<TValue>>();

		// 2
		public void Add(TKey key, TValue value)
		{
			List<TValue> list;
			if (this._dictionary.TryGetValue(key, out list))
			{
				// 2A.
				list.Add(value);
			}
			else
			{
				// 2B.
				list = new List<TValue>();
				list.Add(value);
				this._dictionary[key] = list;
			}
		}

		// 3
		public IEnumerable<TKey> Keys
		{
			get
			{
				return this._dictionary.Keys;
			}
		}

		// 4
		public List<TValue> this[TKey key]
		{
			get
			{
				List<TValue> list;
				if (this._dictionary.TryGetValue(key, out list))
				{
					return list;
				}
				else
				{
					return new List<TValue>();
				}
			}
		}

		public bool ContainsKey(TKey key)
		{
			return _dictionary.ContainsKey(key);
		}

		public void Remove(TKey key)
		{
			_dictionary.Remove(key);
		}


		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			foreach (var listValuePair in _dictionary)
			{
				foreach (TValue value in listValuePair.Value)//remember, the "value" side of the pair is a list of values
				{
					yield return new KeyValuePair<TKey, TValue>(listValuePair.Key,value);
				}
			}
		}


		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public bool MoveNext()
		{
			throw new NotImplementedException();
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}

		public KeyValuePair<TKey, TValue> Current
		{
			get { throw new NotImplementedException(); }
		}

		object IEnumerator.Current
		{
			get { return Current; }
		}
	}
}