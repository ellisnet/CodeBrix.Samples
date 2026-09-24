namespace BrixInvaders.GameLogic;

/// <summary>A meteor of a meteor shower. It damages the player on contact and can be shot.</summary>
public sealed class Meteor
{
    /// <summary>Hit-box size of a big meteor (square).</summary>
    public const double BigSize = 64.0;

    /// <summary>Hit-box size of a small meteor (square).</summary>
    public const double SmallSize = 32.0;

    internal Meteor(int id, MeteorSize size, double x, double y, double vx, double vy, double rotationSpeed)
    {
        Id = id;
        Size = size;
        X = x;
        Y = y;
        Vx = vx;
        Vy = vy;
        RotationSpeed = rotationSpeed;
        Health = size == MeteorSize.Big ? 2 : 1;
    }

    /// <summary>Unique id within the simulation.</summary>
    public int Id { get; }

    /// <summary>Size.</summary>
    public MeteorSize Size { get; }

    /// <summary>Centre x.</summary>
    public double X { get; internal set; }

    /// <summary>Centre y.</summary>
    public double Y { get; internal set; }

    /// <summary>Velocity x.</summary>
    public double Vx { get; }

    /// <summary>Velocity y.</summary>
    public double Vy { get; }

    /// <summary>Current rotation in radians (cosmetic).</summary>
    public double Rotation { get; internal set; }

    /// <summary>Rotation speed in radians per second (cosmetic).</summary>
    public double RotationSpeed { get; }

    /// <summary>Remaining hits.</summary>
    public int Health { get; internal set; }

    /// <summary>True until destroyed or off screen.</summary>
    public bool IsAlive { get; internal set; } = true;

    /// <summary>The hit box.</summary>
    public Box Box => Size == MeteorSize.Big
        ? Box.FromCenter(X, Y, BigSize, BigSize)
        : Box.FromCenter(X, Y, SmallSize, SmallSize);
}
