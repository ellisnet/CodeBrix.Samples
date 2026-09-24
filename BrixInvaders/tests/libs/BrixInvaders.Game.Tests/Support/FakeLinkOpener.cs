using System.Collections.Generic;
using System.Threading.Tasks;
using BrixInvaders.Game.Links;

namespace BrixInvaders.Game.Tests.Support;

/// <summary>Records the links it is asked to open and answers with a fixed result.</summary>
internal sealed class FakeLinkOpener : IExternalLinkOpener
{
    public bool Result { get; set; } = true;

    public List<string> Opened { get; } = new List<string>();

    public Task<bool> Open(string url)
    {
        Opened.Add(url);
        return Task.FromResult(Result);
    }
}
