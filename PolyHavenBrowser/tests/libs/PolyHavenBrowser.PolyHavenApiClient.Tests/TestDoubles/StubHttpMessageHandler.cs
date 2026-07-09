using System.Net;
using System.Text;

namespace PolyHavenBrowser.PolyHavenApiClient.Tests;

/// <summary>
/// A test double for <see cref="HttpMessageHandler"/> that serves canned responses for
/// registered routes and records every request it receives. Unrouted requests get a 404.
/// </summary>
internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly List<(Func<HttpRequestMessage, bool> Match, Func<HttpRequestMessage, HttpResponseMessage> Respond)> _routes = [];

    public List<HttpRequestMessage> Requests { get; } = [];

    public List<string> RequestUris { get; } = [];

    public bool IsDisposed { get; private set; }

    /// <summary>Serves <paramref name="json"/> for requests whose path-and-query matches exactly.</summary>
    public void OnPath(string pathAndQuery, string json, HttpStatusCode statusCode = HttpStatusCode.OK) =>
        _routes.Add((
            request => request.RequestUri!.PathAndQuery == pathAndQuery,
            _ => new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            }));

    /// <summary>Serves <paramref name="json"/> for requests whose absolute URL contains <paramref name="urlFragment"/>.</summary>
    public void OnUrlContains(string urlFragment, string json, HttpStatusCode statusCode = HttpStatusCode.OK) =>
        _routes.Add((
            request => request.RequestUri!.AbsoluteUri.Contains(urlFragment, StringComparison.Ordinal),
            _ => new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            }));

    /// <summary>Serves raw bytes for requests whose absolute URL contains <paramref name="urlFragment"/>.</summary>
    public void OnUrlContainsBytes(string urlFragment, byte[] bytes) =>
        _routes.Add((
            request => request.RequestUri!.AbsoluteUri.Contains(urlFragment, StringComparison.Ordinal),
            _ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) }));

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        RequestUris.Add(request.RequestUri!.AbsoluteUri);

        foreach (var (match, respond) in _routes)
        {
            if (match(request))
            {
                return Task.FromResult(respond(request));
            }
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent($"No stub route registered for {request.RequestUri}"),
        });
    }

    protected override void Dispose(bool disposing)
    {
        IsDisposed = true;
        base.Dispose(disposing);
    }
}
