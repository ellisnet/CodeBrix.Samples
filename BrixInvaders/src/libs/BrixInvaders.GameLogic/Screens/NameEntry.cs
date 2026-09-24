using System;
using System.Text;

namespace BrixInvaders.GameLogic;

/// <summary>
/// Three-letter name entry for keyboard or gamepad: Up/Down cycle the letter under the cursor (wrapping through
/// A..Z then 0..9), Left/Right move the cursor, Confirm moves right and on the last letter completes the entry,
/// Back moves left.
/// </summary>
public sealed class NameEntry
{
    /// <summary>Letters in a name.</summary>
    public const int Length = 3;

    /// <summary>The characters a name may contain, in cycling order.</summary>
    public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    private readonly int[] _letters = new int[Length];

    /// <summary>Creates an entry starting from a name (normalised; "AAA" when empty).</summary>
    /// <param name="initialName">Starting name, e.g. the last name used.</param>
    public NameEntry(string initialName = "AAA")
    {
        var name = Normalize(initialName);
        for (var i = 0; i < Length; i++)
        {
            _letters[i] = Alphabet.IndexOf(name[i]);
        }
    }

    /// <summary>Cursor position, 0..2.</summary>
    public int Cursor { get; private set; }

    /// <summary>True once confirmed on the last letter.</summary>
    public bool IsComplete { get; private set; }

    /// <summary>The name as entered so far.</summary>
    public string Name
    {
        get
        {
            var builder = new StringBuilder(Length);
            foreach (var index in _letters)
            {
                builder.Append(Alphabet[index]);
            }

            return builder.ToString();
        }
    }

    /// <summary>
    /// Normalises any text to a valid three-letter name: upper case, characters outside <see cref="Alphabet"/>
    /// become 'A', padded with 'A' or cut to three.
    /// </summary>
    /// <param name="name">Any text (null allowed).</param>
    /// <returns>A valid name.</returns>
    public static string Normalize(string name)
    {
        var builder = new StringBuilder(Length);
        var text = (name ?? string.Empty).ToUpperInvariant();
        for (var i = 0; i < Length; i++)
        {
            var c = i < text.Length ? text[i] : 'A';
            builder.Append(Alphabet.IndexOf(c) >= 0 ? c : 'A');
        }

        return builder.ToString();
    }

    /// <summary>The letter at a position.</summary>
    /// <param name="position">0..2.</param>
    /// <returns>The letter.</returns>
    public char LetterAt(int position) => Alphabet[_letters[Math.Clamp(position, 0, Length - 1)]];

    /// <summary>Cycles the letter under the cursor forward (Z -> 0, 9 -> A).</summary>
    public void Up() => Cycle(1);

    /// <summary>Cycles the letter under the cursor backward (A -> 9).</summary>
    public void Down() => Cycle(-1);

    /// <summary>Moves the cursor left (stops at 0).</summary>
    public void Left()
    {
        if (!IsComplete && Cursor > 0)
        {
            Cursor--;
        }
    }

    /// <summary>Moves the cursor right (stops at 2).</summary>
    public void Right()
    {
        if (!IsComplete && Cursor < Length - 1)
        {
            Cursor++;
        }
    }

    /// <summary>Moves right, or completes the entry on the last letter.</summary>
    public void Confirm()
    {
        if (IsComplete)
        {
            return;
        }

        if (Cursor < Length - 1)
        {
            Cursor++;
        }
        else
        {
            IsComplete = true;
        }
    }

    /// <summary>Applies one menu input snapshot. Returns true when this input completed the entry.</summary>
    /// <param name="input">The input.</param>
    /// <returns>True when completed by this input.</returns>
    public bool Handle(MenuInput input)
    {
        if (IsComplete)
        {
            return false;
        }

        if (input.Up)
        {
            Up();
        }

        if (input.Down)
        {
            Down();
        }

        if (input.Left || input.Back)
        {
            Left();
        }

        if (input.Right)
        {
            Right();
        }

        if (input.Confirm || input.Start)
        {
            Confirm();
        }

        return IsComplete;
    }

    private void Cycle(int delta)
    {
        if (IsComplete)
        {
            return;
        }

        var count = Alphabet.Length;
        _letters[Cursor] = (((_letters[Cursor] + delta) % count) + count) % count;
    }
}
