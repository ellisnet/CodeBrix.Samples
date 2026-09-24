using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BrixInvaders.Assets;

/// <summary>
/// Reads the constants of <see cref="AssetKeys"/> back out: every asset key, and every atlas frame name with the
/// atlas it belongs to. Used by the start-up description and by the tests that prove every key resolves.
/// </summary>
public static class AssetKeyCatalog
{
    private const string AtlasFieldName = "Atlas";
    private const string KeyPrefix = "kenney:";

    private static readonly Lazy<IReadOnlyList<string>> _allKeys = new(ReadAllKeys);
    private static readonly Lazy<IReadOnlyList<AtlasFrameGroup>> _frameGroups = new(ReadFrameGroups);

    /// <summary>
    /// Gets every distinct asset key in <see cref="AssetKeys"/> (asset groups, the <c>Atlas</c> of every frame group),
    /// in declaration order.
    /// </summary>
    public static IReadOnlyList<string> AllKeys => _allKeys.Value;

    /// <summary>Gets every frame group of <see cref="AssetKeys"/>: its name, its atlas key and its frame names.</summary>
    public static IReadOnlyList<AtlasFrameGroup> FrameGroups => _frameGroups.Value;

    private static IEnumerable<(Type Group, FieldInfo Field, string Value)> Constants() =>
        typeof(AssetKeys).GetNestedTypes(BindingFlags.Public)
            .SelectMany(group => group
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.IsLiteral && field.FieldType == typeof(string))
                .Select(field => (group, field, (string)field.GetRawConstantValue())));

    private static IReadOnlyList<string> ReadAllKeys() =>
        Constants()
            .Where(constant => constant.Value.StartsWith(KeyPrefix, StringComparison.Ordinal))
            .Select(constant => constant.Value)
            .Distinct(StringComparer.Ordinal)
            .ToList();

    private static IReadOnlyList<AtlasFrameGroup> ReadFrameGroups() =>
        Constants()
            .GroupBy(constant => constant.Group)
            .Where(group => group.Any(constant => constant.Field.Name == AtlasFieldName))
            .Select(group => new AtlasFrameGroup(
                group.Key.Name,
                group.Single(constant => constant.Field.Name == AtlasFieldName).Value,
                group.Where(constant => constant.Field.Name != AtlasFieldName)
                    .Select(constant => constant.Value)
                    .Distinct(StringComparer.Ordinal)
                    .ToList()))
            .ToList();
}

/// <summary>One frame group of <see cref="AssetKeys"/>: the frames a game cuts from one atlas.</summary>
/// <param name="GroupName">The nested class name, for example <c>Enemies</c>.</param>
/// <param name="AtlasKey">The asset key of the atlas the frames live in.</param>
/// <param name="FrameNames">The frame (region) names inside that atlas.</param>
public sealed record AtlasFrameGroup(string GroupName, string AtlasKey, IReadOnlyList<string> FrameNames);
