namespace BrixInvaders.GameLogic;

/// <summary>Where an enemy is relative to its formation slot.</summary>
public enum EnemyState
{
    /// <summary>Sitting in its formation slot and moving with the formation.</summary>
    InFormation = 0,

    /// <summary>Flying its dive curve.</summary>
    Diving = 1,

    /// <summary>Flying back to its (moving) formation slot.</summary>
    Returning = 2,
}
