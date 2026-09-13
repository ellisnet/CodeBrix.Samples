// BoundedCacheTests.cs
//
// The icon caches sit behind this type, so its eviction rule is what stops a
// long session from growing them forever. Eviction is invisible at run time -
// a dropped icon is simply rendered again - which is exactly why it is worth
// pinning down here.

using System;
using Pinta.Brix.Controls;
using SilverAssertions;
using Xunit;

namespace Pinta.Brix.Controls.Tests;

public class BoundedCacheTests
{
	[Fact]
	public void TryGetValue_returns_what_was_stored ()
	{
		//Arrange
		BoundedCache<string, int> cache = new (4);

		//Act
		cache.Set ("a", 1);
		bool found = cache.TryGetValue ("a", out int value);

		//Assert
		found.Should ().BeTrue ();
		value.Should ().Be (1);
	}

	[Fact]
	public void TryGetValue_reports_a_missing_key ()
	{
		//Arrange
		BoundedCache<string, int> cache = new (4);

		//Act
		bool found = cache.TryGetValue ("missing", out int value);

		//Assert
		found.Should ().BeFalse ();
		value.Should ().Be (0);
	}

	[Fact]
	public void Set_replaces_an_existing_value_without_growing ()
	{
		//Arrange
		BoundedCache<string, int> cache = new (4);
		cache.Set ("a", 1);

		//Act
		cache.Set ("a", 2);
		cache.TryGetValue ("a", out int value);

		//Assert
		cache.Count.Should ().Be (1);
		value.Should ().Be (2);
	}

	[Fact]
	public void Set_never_holds_more_than_the_capacity ()
	{
		//Arrange
		BoundedCache<int, int> cache = new (3);

		//Act
		for (int i = 0; i < 50; i++)
			cache.Set (i, i);

		//Assert
		cache.Count.Should ().Be (3);
	}

	[Fact]
	public void Set_evicts_the_least_recently_used_entry ()
	{
		//Arrange
		BoundedCache<string, int> cache = new (2);
		cache.Set ("a", 1);
		cache.Set ("b", 2);

		//Act
		//"a" is used again, so "b" becomes the least recently used one.
		cache.TryGetValue ("a", out _);
		cache.Set ("c", 3);

		//Assert
		cache.TryGetValue ("a", out _).Should ().BeTrue ();
		cache.TryGetValue ("b", out _).Should ().BeFalse ();
		cache.TryGetValue ("c", out _).Should ().BeTrue ();
	}

	[Fact]
	public void Clear_empties_the_cache ()
	{
		//Arrange
		BoundedCache<string, int> cache = new (4);
		cache.Set ("a", 1);
		cache.Set ("b", 2);

		//Act
		cache.Clear ();

		//Assert
		cache.Count.Should ().Be (0);
		cache.TryGetValue ("a", out _).Should ().BeFalse ();
	}

	[Theory]
	[InlineData (0)]
	[InlineData (-1)]
	public void Constructor_refuses_a_capacity_below_one (int capacity)
	{
		//Act
		Action create = () => _ = new BoundedCache<string, int> (capacity);

		//Assert
		Assert.Throws<ArgumentOutOfRangeException> (create);
	}
}
