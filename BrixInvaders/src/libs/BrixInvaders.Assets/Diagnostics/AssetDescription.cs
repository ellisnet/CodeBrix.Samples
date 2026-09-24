using CodeBrix.Platform.GameEngine.Assets.Providers;

namespace BrixInvaders.Assets;

/// <summary>What one asset key resolved to in the provider's catalog (nothing is materialized to find out).</summary>
/// <param name="Key">The asset key asked for.</param>
/// <param name="Found">Whether the provider knows the key.</param>
/// <param name="Kind">The asset kind; <see cref="GameAssetKind.Unknown"/> when not found.</param>
/// <param name="SizeBytes">The size of the asset's file inside its zip; 0 when not found.</param>
/// <param name="Detail">Extra facts, such as an atlas's frame count; empty when there are none.</param>
public sealed record AssetDescription(string Key, bool Found, GameAssetKind Kind, long SizeBytes, string Detail);
