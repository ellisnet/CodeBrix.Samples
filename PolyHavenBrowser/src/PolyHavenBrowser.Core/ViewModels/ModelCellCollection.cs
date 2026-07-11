using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PolyHavenBrowser.PolyHavenApiClient;

// ReSharper disable once CheckNamespace
namespace PolyHavenBrowser.ViewModels;

/// <summary>
/// The Browsing View's lazily-loading item source: it holds the full (sorted, filtered)
/// model list but only materializes <see cref="ModelCellViewModel"/> cells in batches - a
/// first screenful up front, then more whenever the catalog grid scrolls near its bottom
/// edge (the page watches the ScrollViewer and calls <see cref="RequestMore"/>) - so
/// hundreds of cells and thumbnails are never created before they can be seen.
/// </summary>
#if HAS_CODEBRIX
[Microsoft.UI.Xaml.Data.Bindable]
#endif
public class ModelCellCollection : ObservableCollection<ModelCellViewModel>
{
    //Enough cells to overfill the first screen even on a wide monitor.
    private const int InitialBatch = 30;

    private readonly IReadOnlyList<PolyHavenAsset> _source;
    private readonly Func<PolyHavenAsset, ModelCellViewModel> _cellFactory;

    /// <summary>Creates the collection over an already sorted and filtered model list.</summary>
    public ModelCellCollection(
        IReadOnlyList<PolyHavenAsset> source, Func<PolyHavenAsset, ModelCellViewModel> cellFactory)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _cellFactory = cellFactory ?? throw new ArgumentNullException(nameof(cellFactory));

        RequestMore(InitialBatch);
    }

    /// <summary>The total number of models behind the collection (materialized or not).</summary>
    public int TotalCount => _source.Count;

    /// <summary>Whether models remain that have no materialized cell yet.</summary>
    public bool HasMoreItems => Count < _source.Count;

    /// <summary>
    /// Materializes up to <paramref name="count"/> further cells (each one starts fetching
    /// its thumbnail as it is created). Safe to call repeatedly; extra calls simply no-op
    /// once every model has its cell.
    /// </summary>
    public void RequestMore(int count)
    {
        var toLoad = Math.Min(count, _source.Count - Count);

        for (var i = 0; i < toLoad; i++)
        {
            var cell = _cellFactory(_source[Count]);
            Add(cell);

            //Fire-and-forget: the cell fetches its thumbnail in the background and raises
            //a property change when the image arrives.
            _ = cell.LoadThumbnailAsync();
        }
    }
}
