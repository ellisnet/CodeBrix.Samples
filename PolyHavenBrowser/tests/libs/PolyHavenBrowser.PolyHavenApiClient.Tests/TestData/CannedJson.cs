namespace PolyHavenBrowser.PolyHavenApiClient.Tests;

/// <summary>
/// Canned Poly Haven API responses (trimmed copies of real payloads) for offline tests.
/// </summary>
internal static class CannedJson
{
    public const string Types = """["hdris","textures","models"]""";

    public const string Assets = """
        {
          "abandoned_bakery": {
            "type": 0,
            "name": "Abandoned Bakery",
            "categories": ["natural light", "urban", "indoor"],
            "tags": ["abandoned", "brick"],
            "authors": {"Sergej Majboroda": "All"},
            "description": "Free 16K HDRI of an abandoned bakery.",
            "date_published": 1663804800,
            "date_taken": 1662805680,
            "download_count": 18040,
            "files_hash": "d81af70dd51ebb704af086506e0a9b92bb5d7b84",
            "thumbnail_url": "https://cdn.polyhaven.com/asset_img/thumbs/abandoned_bakery.png?width=256&height=256",
            "max_resolution": [16384, 8192],
            "coords": [50.786873, 34.774073],
            "backplates": true,
            "evs_cap": 16,
            "whitebalance": 4950,
            "attributes": {"contrast": "high", "indoor": true},
            "category": "Abandoned & Ruins/Buildings/Derelict Interiors",
            "sponsors": ["7934315"]
          },
          "aerial_rocks_02": {
            "type": 1,
            "name": "Aerial Rocks 02",
            "categories": ["terrain", "outdoor", "rock"],
            "tags": ["rocks", "cliff"],
            "authors": {"Rob Tuytel": "All"},
            "date_published": 1600274572,
            "download_count": 51234,
            "dimensions": [50000, 50000],
            "max_resolution": [8192, 8192],
            "thumbnail_url": "https://cdn.polyhaven.com/asset_img/thumbs/aerial_rocks_02.png?width=256&height=256"
          }
        }
        """;

    public const string AssetInfo = """
        {
          "type": 0,
          "name": "Abandoned Bakery",
          "categories": ["natural light", "urban", "indoor"],
          "tags": ["abandoned", "brick"],
          "authors": {"Sergej Majboroda": "All"},
          "description": "Free 16K HDRI of an abandoned bakery.",
          "date_published": 1663804800,
          "date_taken": 1662805680,
          "download_count": 18040,
          "files_hash": "d81af70dd51ebb704af086506e0a9b92bb5d7b84",
          "thumbnail_url": "https://cdn.polyhaven.com/asset_img/thumbs/abandoned_bakery.png?width=256&height=256",
          "max_resolution": [16384, 8192],
          "coords": [50.786873, 34.774073],
          "backplates": true,
          "evs_cap": 16,
          "whitebalance": 4950,
          "attributes": {"contrast": "high", "indoor": true},
          "category": "Abandoned & Ruins/Buildings/Derelict Interiors"
        }
        """;

    public const string Author = """
        {
          "name": "Sergej Majboroda",
          "link": "https://hdrmarket.com/",
          "encryptedEmail": {"iv": "94c50e70", "content": "0cb68e53"}
        }
        """;

    public const string Categories = """
        {"all": 978, "natural light": 805, "outdoor": 689, "skies": 294}
        """;

    public const string HdriFiles = """
        {
          "hdri": {
            "1k": {
              "hdr": {"size": 1730263, "md5": "9a835b2f0e7a42e2b98fefd19c4a3b9d", "url": "https://dl.polyhaven.org/file/ph-assets/HDRIs/hdr/1k/abandoned_bakery_1k.hdr"},
              "exr": {"size": 1716522, "md5": "791b0de5f359fdb251bc19fadd6f567f", "url": "https://dl.polyhaven.org/file/ph-assets/HDRIs/exr/1k/abandoned_bakery_1k.exr"}
            },
            "4k": {
              "hdr": {"size": 26680120, "md5": "8cd8c249277a987626b816c5d9df8b3b", "url": "https://dl.polyhaven.org/file/ph-assets/HDRIs/hdr/4k/abandoned_bakery_4k.hdr"}
            }
          },
          "tonemapped": {"size": 48676915, "md5": "700b8d3b60ae0d3eeeddb17577b7457a", "url": "https://dl.polyhaven.org/file/ph-assets/HDRIs/extra/Tonemapped%20JPG/abandoned_bakery.jpg"}
        }
        """;

    public const string TextureFiles = """
        {
          "blend": {
            "blend": {
              "size": 100200,
              "md5": "aa0000000000000000000000000000aa",
              "url": "https://dl.polyhaven.org/file/ph-assets/Textures/blend/1k/aerial_rocks_02.blend",
              "include": {
                "textures/aerial_rocks_02_diff_1k.jpg": {"size": 785923, "md5": "b5795e9b9e43d718ace73448c97d57ab", "url": "https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/aerial_rocks_02/aerial_rocks_02_diff_1k.jpg"},
                "textures/aerial_rocks_02_nor_dx_1k.jpg": {"size": 123456, "md5": "cc0000000000000000000000000000cc", "url": "https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/aerial_rocks_02/aerial_rocks_02_nor_dx_1k.jpg"}
              }
            }
          },
          "Diffuse": {
            "1k": {
              "jpg": {"size": 785923, "md5": "b5795e9b9e43d718ace73448c97d57ab", "url": "https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/aerial_rocks_02/aerial_rocks_02_diff_1k.jpg"}
            }
          }
        }
        """;
}
