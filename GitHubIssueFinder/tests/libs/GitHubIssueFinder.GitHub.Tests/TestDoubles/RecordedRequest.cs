using System;
using System.Collections.Generic;
using System.Net.Http;

namespace GitHubIssueFinder.GitHub.Tests;

//What the stub handler saw of one request. Kept as plain values because the request message
//itself is disposed the moment the call returns.
internal sealed class RecordedRequest
{
    private RecordedRequest(string method, string url, string pathAndQuery,
        IReadOnlyDictionary<string, string> headers)
    {
        Method = method;
        Url = url;
        PathAndQuery = pathAndQuery;
        Headers = headers;
    }

    internal string Method { get; }

    internal string Url { get; }

    internal string PathAndQuery { get; }

    internal IReadOnlyDictionary<string, string> Headers { get; }

    internal string Header(string name) =>
        Headers.TryGetValue(name, out var value) ? value : null;

    internal static RecordedRequest From(HttpRequestMessage request)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in request.Headers)
        {
            headers[header.Key] = string.Join(", ", header.Value);
        }

        return new RecordedRequest(
            request.Method.Method,
            request.RequestUri.AbsoluteUri,
            request.RequestUri.PathAndQuery,
            headers);
    }
}
