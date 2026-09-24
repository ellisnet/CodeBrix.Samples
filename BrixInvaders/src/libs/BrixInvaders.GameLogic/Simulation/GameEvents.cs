using System.Collections;
using System.Collections.Generic;

namespace BrixInvaders.GameLogic;

/// <summary>The events of the most recent simulation step. Cleared at the start of every step.</summary>
public sealed class GameEvents : IReadOnlyList<GameEvent>
{
    private readonly List<GameEvent> _items = new List<GameEvent>();

    /// <inheritdoc />
    public int Count => _items.Count;

    /// <inheritdoc />
    public GameEvent this[int index] => _items[index];

    /// <summary>True when at least one event of the kind happened.</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>True when present.</returns>
    public bool Contains(GameEventKind kind) => CountOf(kind) > 0;

    /// <summary>How many events of the kind happened.</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>The count.</returns>
    public int CountOf(GameEventKind kind)
    {
        var count = 0;
        foreach (var item in _items)
        {
            if (item.Kind == kind)
            {
                count++;
            }
        }

        return count;
    }

    /// <inheritdoc />
    public IEnumerator<GameEvent> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal void Add(GameEvent item) => _items.Add(item);

    internal void Add(GameEventKind kind) => _items.Add(new GameEvent(kind));

    internal void Clear() => _items.Clear();
}
