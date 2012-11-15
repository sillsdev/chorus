#region Copyright  2004-06 FocusPoint Solutions
// This File is part of the SpatialSharp Project
//
// Copyright  2004-06 FocusPoint Solutions
// All rights reserved

// This library is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published
// by the Free Software Foundation; either version 2.1 of the License, or
// (at your option) any later version.
//
// This library is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this library; if not, write to the
// Free Software Foundation, Inc.,
// 51 Franklin Street, Fifth Floor Boston, MA  02110-1301 USA
#endregion

#region Revision history
// 2004.04.26, Pedro Gomes (pedro.gomes@focuspoint-solutions.com) -
//      Initial version.
#endregion

#region Include directives

using System;
using System.Collections;

#endregion

namespace Chorus.merge.xml.generic {
  /// <summary>
  /// </summary>
  public class MergeSort {
	#region Constants
	#endregion // Constants

	#region Internals
	#region Member Variables
	#endregion  // Member Variables

	#region Internal Properties
	#endregion // Internal Properties

	#region Internal Methods
	private static void InternalSort(Array array, int startIndex, int endIndex, IComparer comparer) {
	  if (startIndex == endIndex) {
		return; // nothing to do
	  }

	  // recurse sorting
	  int length = endIndex - startIndex + 1;
	  int pivot = (startIndex + endIndex) / 2;
	  InternalSort(array, startIndex, pivot, comparer);
	  InternalSort(array, pivot + 1, endIndex, comparer);

	  // execute merge sort
	  object[] working = new object[length];
	  Array.Copy(array, startIndex, working, 0, length);

	  int m1 = 0;
	  int m2 = pivot - startIndex + 1;

	  for (int i = 0; i < length; ++i) {
		if (m2 <= (endIndex - startIndex)) {
		  if (m1 <= (pivot - startIndex)) {
			if (0 < comparer.Compare(working[m1], working[m2])) {
			  array.SetValue(working[m2++], startIndex + i);
			} else {
			  array.SetValue(working[m1++], startIndex + i);
			}
		  } else {
			array.SetValue(working[m2++], startIndex + i);
		  }
		} else {
		  array.SetValue(working[m1++], startIndex + i);
		}
	  }
	}
	#endregion // Internal Methods
	#endregion // Internals

	#region Constructors
	#endregion // Constructors

	#region Properties
	#endregion // Properties

	#region Operators
	#endregion // Operators

	#region Events
	#endregion // Events

	#region Methods
	/// <summary>
	/// Sorts the specified array.
	/// </summary>
	/// <param name="array">The array.</param>
	public static void Sort(Array array) {
	  Sort(array, Comparer.Default);
	}

	/// <summary>
	/// Sorts the specified array.
	/// </summary>
	/// <param name="array">The array.</param>
	/// <param name="comparer">The comparer.</param>
	public static void Sort(Array array, IComparer comparer) {
	  Sort(array, 0, array.GetLength(0), comparer);
	}

	/// <summary>
	/// Sorts the specified array.
	/// </summary>
	/// <param name="array">The array.</param>
	/// <param name="index">The index.</param>
	/// <param name="length">The length.</param>
	public static void Sort(Array array, int index, int length) {
	  Sort(array, index, length, Comparer.Default);
	}

	/// <summary>
	/// Sorts the specified array.
	/// </summary>
	/// <param name="array">The array.</param>
	/// <param name="index">The index.</param>
	/// <param name="length">The length.</param>
	/// <param name="comparer">The comparer.</param>
	public static void Sort(Array array, int index, int length, IComparer comparer) {
	  if (null == array) {
		throw new ArgumentNullException("array");
	  }

	  if (1 != array.Rank) {
		throw new RankException();
	  }

	  if (0 > length) {
		throw new ArgumentOutOfRangeException("length");
	  }

	  if ((array.Length - length) < index) {
		throw new ArgumentOutOfRangeException("index");
	  }

	  if (length <= 1) {
		return; // nothing to do
	  }

	  InternalSort(array, index, (index + length) - 1, comparer);
	}
	#endregion // Methods
  }
}