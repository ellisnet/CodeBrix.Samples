using System;
using System.Collections.Generic;
using System.Linq;
using CodeBrix.Platform.GameEngine;
using CodeBrix.Platform.GameEngine.Assets.Providers;
using CodeBrix.Platform.GameEngine.KenneyAssets.Sources;

namespace BrixInvaders.Assets;

/// <summary>Describes what every key in <see cref="AssetKeys"/> resolved to, for the start-up log.</summary>
public static class AssetReport
{
    /// <summary>Describes every key in <see cref="AssetKeyCatalog.AllKeys"/>.</summary>
    /// <param name="engine">The engine the packs are registered with.</param>
    /// <returns>One description per key, in declaration order.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> is null.</exception>
    public static IReadOnlyList<AssetDescription> DescribeAll(Engine engine) => Describe(engine, AssetKeyCatalog.AllKeys);

    /// <summary>Describes a set of asset keys.</summary>
    /// <param name="engine">The engine the packs are registered with.</param>
    /// <param name="keys">The keys.</param>
    /// <returns>One description per key, in the order given.</returns>
    /// <exception cref="ArgumentNullException">Thrown when an argument is null.</exception>
    public static IReadOnlyList<AssetDescription> Describe(Engine engine, IEnumerable<string> keys)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(keys);

        GameAssetProviderRegistry providers = engine.Managers.AssetProviders;
        List<AssetDescription> result = [];
        foreach (string key in keys)
        {
            if (providers.TryDescribe(key, out GameAssetDescriptor descriptor) && descriptor is not null)
            {
                string detail = descriptor.Properties.TryGetValue(KenneyAssetProperties.AtlasFrameCount, out string frames)
                    ? $"{frames} frames"
                    : string.Empty;
                result.Add(new AssetDescription(key, true, descriptor.Kind, descriptor.SizeBytes, detail));
            }
            else
            {
                result.Add(new AssetDescription(key, false, GameAssetKind.Unknown, 0, string.Empty));
            }
        }

        return result;
    }

    /// <summary>Builds the one start-up line summarising a set of descriptions.</summary>
    /// <param name="descriptions">The descriptions.</param>
    /// <returns>
    /// For example <c>[BrixInvaders] assets: 72 key(s) described - Audio 36, Font 2, ... - 0 missing, 9.1 MB</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="descriptions"/> is null.</exception>
    public static string Summary(IReadOnlyList<AssetDescription> descriptions)
    {
        ArgumentNullException.ThrowIfNull(descriptions);

        string byKind = string.Join(
            ", ",
            descriptions
                .Where(description => description.Found)
                .GroupBy(description => description.Kind)
                .OrderBy(group => group.Key.ToString(), StringComparer.Ordinal)
                .Select(group => $"{group.Key} {group.Count()}"));
        int missing = descriptions.Count(description => !description.Found);
        double megabytes = descriptions.Sum(description => description.SizeBytes) / (1024.0 * 1024.0);

        return $"{BrixInvadersAssets.LogPrefix} {descriptions.Count} key(s) described - {byKind} - " +
               $"{missing} missing, {megabytes:0.0} MB";
    }

    /// <summary>
    /// Describes every key and logs the summary line, then one line per key that did not resolve.
    /// </summary>
    /// <param name="engine">The engine the packs are registered with.</param>
    /// <param name="log">An extra receiver for the lines, or <see langword="null"/>.</param>
    /// <returns>The descriptions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> is null.</exception>
    public static IReadOnlyList<AssetDescription> LogAll(Engine engine, Action<string> log = null)
    {
        IReadOnlyList<AssetDescription> descriptions = DescribeAll(engine);
        BrixInvadersAssets.Write(Summary(descriptions), log);
        foreach (AssetDescription missing in descriptions.Where(description => !description.Found))
        {
            BrixInvadersAssets.Write($"{BrixInvadersAssets.LogPrefix} WARNING: key not found: {missing.Key}", log);
        }

        return descriptions;
    }
}
