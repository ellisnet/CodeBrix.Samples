using System.Collections.Generic;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Engine;

namespace GoddessTempleDiscovery.Rules.Brains;

/// <summary>How one temperament values the things a turn can gain or spend.</summary>
/// <param name="WorkerShallow">The cost of a Worker spent on a site of tiers 1 to 6.</param>
/// <param name="WorkerDeep">The cost of a Worker spent on a site of tiers 7 to 9.</param>
/// <param name="TabletCost">The cost of spending a Tablet on a dig.</param>
/// <param name="Star">The extra value of a star-marked Discovery.</param>
/// <param name="DeepTier">The extra value of a tier 7 to 9 Discovery.</param>
/// <param name="ShallowTier">The extra value of a tier 1 to 4 Discovery.</param>
/// <param name="SamePeriod">The extra value of a Discovery whose period is already in the hand (stratigraphy).</param>
/// <param name="AdjacentPeriod">The extra value of a Discovery next to a period in the hand (sequence).</param>
/// <param name="Study">The extra value of each Tablet drawn by Study.</param>
/// <param name="PublishStratigraphy">The extra value of publishing a stratigraphy report.</param>
/// <param name="PublishSequence">The extra value of publishing a sequence report.</param>
/// <param name="Roles">The value of recruiting each Specialist role.</param>
public sealed record TemperamentWeights(
    double WorkerShallow,
    double WorkerDeep,
    double TabletCost,
    double Star,
    double DeepTier,
    double ShallowTier,
    double SamePeriod,
    double AdjacentPeriod,
    double Study,
    double PublishStratigraphy,
    double PublishSequence,
    IReadOnlyDictionary<SpecialistRole, double> Roles)
{
    /// <summary>Steady shallow digs, keeps Tablets, publishes stratigraphy reports.</summary>
    public static TemperamentWeights Surveyor { get; } = new TemperamentWeights(
        0.7, 0.7, 1.6, 0.5, 0.0, 0.5, 0.6, 0.1, 0.2, 1.5, 0.5,
        new Dictionary<SpecialistRole, double>
        {
            [SpecialistRole.Surveyor] = 2.5,
            [SpecialistRole.SmallFindsKeeper] = 2.2,
            [SpecialistRole.Architect] = 2.0,
            [SpecialistRole.Foreman] = 2.0,
            [SpecialistRole.Photographer] = 1.5,
            [SpecialistRole.Epigrapher] = 1.2,
        });

    /// <summary>Banks Workers for tiers 7 to 9, spends Tablets freely, chases Her stars.</summary>
    public static TemperamentWeights DeepDigger { get; } = new TemperamentWeights(
        1.5, 0.3, 0.8, 1.5, 1.5, -0.3, 0.2, 0.2, 0.0, 0.5, 0.5,
        new Dictionary<SpecialistRole, double>
        {
            [SpecialistRole.Foreman] = 2.6,
            [SpecialistRole.Architect] = 2.2,
            [SpecialistRole.SmallFindsKeeper] = 1.8,
            [SpecialistRole.Epigrapher] = 1.4,
            [SpecialistRole.Photographer] = 1.2,
            [SpecialistRole.Surveyor] = 0.8,
        });

    /// <summary>Studies, recruits the Epigrapher and the Photographer, publishes sequence reports.</summary>
    public static TemperamentWeights Scholar { get; } = new TemperamentWeights(
        0.8, 0.6, 1.4, 1.0, 0.8, 0.0, 0.2, 0.7, 0.8, 0.5, 2.0,
        new Dictionary<SpecialistRole, double>
        {
            [SpecialistRole.Epigrapher] = 3.0,
            [SpecialistRole.Photographer] = 3.0,
            [SpecialistRole.Architect] = 1.4,
            [SpecialistRole.Foreman] = 1.4,
            [SpecialistRole.SmallFindsKeeper] = 1.2,
            [SpecialistRole.Surveyor] = 1.0,
        });

    /// <summary>The weights of a temperament.</summary>
    /// <param name="temperament">The temperament.</param>
    /// <returns>The weights.</returns>
    public static TemperamentWeights For(Temperament temperament) => temperament switch
    {
        Temperament.DeepDigger => DeepDigger,
        Temperament.Scholar => Scholar,
        _ => Surveyor,
    };

    /// <summary>The value of recruiting a role (0 for an unknown role).</summary>
    /// <param name="role">The role.</param>
    /// <returns>The value.</returns>
    public double Role(SpecialistRole role) => Roles.TryGetValue(role, out var v) ? v : 0.0;
}
