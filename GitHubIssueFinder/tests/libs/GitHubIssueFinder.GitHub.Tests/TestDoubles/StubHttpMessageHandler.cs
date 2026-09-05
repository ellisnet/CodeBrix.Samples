using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GitHubIssueFinder.GitHub.Tests;

//The network stands still for the tests: routes are keyed by the address relative to the
//base address, and anything not routed comes back as a 404 naming what was asked for.
//Registering the same address twice queues the answers, so a refusal can be followed by a
//success without any other machinery. Every request is recorded so a test can read the
//headers and the exact query string that went out.
internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, Queue<StubResponse>> _routes =
        new Dictionary<string, Queue<StubResponse>>(StringComparer.Ordinal);

    internal List<RecordedRequest> Requests { get; } = new List<RecordedRequest>();

    //Proves the point of the internal constructor: the service must never dispose a handler
    //it did not create.
    internal bool IsDisposed { get; private set; }

    internal int RequestCount => Requests.Count;

    internal void Respond(string pathAndQuery, string json,
        HttpStatusCode statusCode = HttpStatusCode.OK, IDictionary<string, string> headers = null)
    {
        var key = Normalize(pathAndQuery);
        if (!_routes.TryGetValue(key, out var queue))
        {
            queue = new Queue<StubResponse>();
            _routes[key] = queue;
        }

        queue.Enqueue(new StubResponse(statusCode, json, headers));
    }

    internal IReadOnlyList<string> PathsCalled()
    {
        var paths = new List<string>(Requests.Count);
        foreach (var request in Requests) { paths.Add(Normalize(request.PathAndQuery)); }
        return paths;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (Requests) { Requests.Add(RecordedRequest.From(request)); }

        var key = Normalize(request.RequestUri.PathAndQuery);
        if (_routes.TryGetValue(key, out var queue) && queue.Count > 0)
        {
            //The last answer registered for an address stays in place, so a test only has to
            //queue the answers that differ.
            var stub = queue.Count > 1 ? queue.Dequeue() : queue.Peek();
            return Task.FromResult(stub.Build());
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(
                "{\"message\":\"No stub route registered for " + request.RequestUri.AbsoluteUri + "\"}",
                Encoding.UTF8, "application/json"),
        });
    }

    protected override void Dispose(bool disposing)
    {
        IsDisposed = true;
        base.Dispose(disposing);
    }

    private static string Normalize(string pathAndQuery)
    {
        if (string.IsNullOrEmpty(pathAndQuery)) { return string.Empty; }
        return pathAndQuery[0] == '/' ? pathAndQuery.Substring(1) : pathAndQuery;
    }

    private sealed class StubResponse
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _json;
        private readonly IDictionary<string, string> _headers;

        internal StubResponse(HttpStatusCode statusCode, string json, IDictionary<string, string> headers)
        {
            _statusCode = statusCode;
            _json = json ?? string.Empty;
            _headers = headers;
        }

        internal HttpResponseMessage Build()
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_json, Encoding.UTF8, "application/json"),
            };

            if (_headers != null)
            {
                foreach (var header in _headers)
                {
                    response.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return response;
        }
    }
}
