using System;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Linq.Expressions;

/// <summary>
/// Represents a catalog after a filter function is applied to it.
/// </summary>
/// <remarks>The code comes from https://github.com/microsoftarchive/mef/blob/master/Wiki/Filtering%20Catalogs.md.
/// Newer versions of .NET (>= 4.5) come with a FilteredCatalog class</remarks>
public class FilteredCatalog : ComposablePartCatalog, INotifyComposablePartCatalogChanged
{
	private readonly ComposablePartCatalog _inner;
	private readonly INotifyComposablePartCatalogChanged _innerNotifyChange;
	private readonly IQueryable<ComposablePartDefinition> _partsQuery;

	/// <summary>
	/// Initializes a new instance of the FilteredCatalog class with the specified underlying
	/// catalog and filter.
	/// </summary>
	public FilteredCatalog(ComposablePartCatalog inner,
		Expression<Func<ComposablePartDefinition, bool>> expression)
	{
		_inner = inner;
		_innerNotifyChange = inner as INotifyComposablePartCatalogChanged;
		_partsQuery = inner.Parts.Where(expression);
	}

	/// <summary>
	/// Gets the part definitions that are contained in the catalog.
	/// </summary>
	public override IQueryable<ComposablePartDefinition> Parts
	{
		get
		{
			return _partsQuery;
		}
	}

	/// <summary>
	/// Occurs when the underlying catalog has changed.
	/// </summary>
	public event EventHandler<ComposablePartCatalogChangeEventArgs> Changed
	{
		add
		{
			if (_innerNotifyChange != null)
				_innerNotifyChange.Changed += value;
		}
		remove
		{
			if (_innerNotifyChange != null)
				_innerNotifyChange.Changed -= value;
		}
	}

	/// <summary>
	/// Occurs when the underlying catalog is changing.
	/// </summary>
	public event EventHandler<ComposablePartCatalogChangeEventArgs> Changing
	{
		add
		{
			if (_innerNotifyChange != null)
				_innerNotifyChange.Changing += value;
		}
		remove
		{
			if (_innerNotifyChange != null)
				_innerNotifyChange.Changing -= value;
		}
	}
}