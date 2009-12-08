using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace Chorus.Utilities
{
	/// <summary>
	/// Initially from http://dotnetperls.com/multimap
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public class MultiMap<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>> where TValue:class
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

		public void RemoveAllItemsWithKey(TKey key)
		{
			_dictionary.Remove(key);
		}

		public void RemoveKeyItemPair(TKey key, TValue value)
		{
			if (!_dictionary.ContainsKey(key))
			{
				throw new ArgumentException("The multimap has no entries for the key '" +key+"'");
			}

			List<TValue> values = _dictionary[key];
			var matches = values.Where(v=>v.Equals(value));
			if(matches==null || matches.Count()==0)
			{
				throw new ArgumentException("The multimap did not find the value '"+values.ToString()+"' under the key '" +key+"'");
			}
			Debug.Assert(matches.Count() == 1);

			//nb: it doesn't make a lot of sense to have it in there more than once, but it's safer to remove them all, if that should happen
			TValue[] doomed = matches.ToArray();

			foreach (var v in doomed)
			{
				values.Remove(v);
			}
			if(values.Count ==0)
			{   //no more matches for this key
				_dictionary.Remove(key);
			}
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