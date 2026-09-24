using System;

namespace BrixInvaders.GameLogic;

/// <summary>The behaviours active in a sector. Each sector design adds one and keeps the earlier ones.</summary>
[Flags]
public enum SectorFeatures
{
    /// <summary>Nothing.</summary>
    None = 0,

    /// <summary>The UFO bonus ship crosses the top of the playfield (sector 1 onwards).</summary>
    Ufo = 1,

    /// <summary>Divers leave the formation, swoop and return (sector 2 onwards).</summary>
    Divers = 2,

    /// <summary>Shielded enemies that need two hits (sector 3 onwards).</summary>
    Shielded = 4,

    /// <summary>Meteor showers between waves (sector 3 onwards).</summary>
    MeteorShowers = 8,

    /// <summary>Missile carriers fire homing missiles (sector 4 onwards).</summary>
    HomingMissiles = 16,

    /// <summary>The formation splits into two independently stepping groups (sector 5).</summary>
    SplitFormation = 32,
}
