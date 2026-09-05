using System;
using System.Collections.Generic;
using System.Globalization;

namespace GitHubIssueFinder.GitHub.Tests;

//Small, obviously-shaped GitHub bodies for the tests that are about counting pages rather
//than about parsing. The saved fixtures cover the parsing.
internal static class JsonBuilders
{
    internal static string SearchPage(int totalCount, int itemCount, string repository = "acme/widgets",
        int firstNumber = 1, bool incompleteResults = false)
    {
        var items = new List<string>(itemCount);
        for (var index = 0; index < itemCount; index++)
        {
            items.Add(SearchItem(firstNumber + index, repository));
        }

        return "{\"total_count\":" + totalCount.ToString(CultureInfo.InvariantCulture)
            + ",\"incomplete_results\":" + (incompleteResults ? "true" : "false")
            + ",\"items\":[" + string.Join(",", items) + "]}";
    }

    internal static string SearchItem(int number, string repository)
    {
        var text = number.ToString(CultureInfo.InvariantCulture);
        return "{\"id\":" + text
            + ",\"number\":" + text
            + ",\"title\":\"Item " + text + "\""
            + ",\"html_url\":\"https://github.com/" + repository + "/issues/" + text + "\""
            + ",\"repository_url\":\"https://api.github.com/repos/" + repository + "\""
            + ",\"state\":\"open\""
            + ",\"user\":{\"login\":\"someone\"}"
            + ",\"created_at\":\"2026-09-01T00:00:00Z\""
            + ",\"updated_at\":\"2026-09-02T00:00:00Z\""
            + ",\"closed_at\":null"
            + ",\"comments\":0"
            + ",\"assignees\":[]"
            + ",\"milestone\":null"
            + ",\"labels\":[]}";
    }

    internal static string Repositories(params string[] fullNames) =>
        Repositories(fullNames, archived: false);

    internal static string Repositories(string[] fullNames, bool archived)
    {
        var entries = new List<string>(fullNames.Length);
        foreach (var fullName in fullNames)
        {
            var name = fullName.Substring(fullName.IndexOf('/') + 1);
            entries.Add("{\"name\":\"" + name + "\",\"full_name\":\"" + fullName
                + "\",\"html_url\":\"https://github.com/" + fullName + "\""
                + ",\"fork\":false,\"archived\":" + (archived ? "true" : "false")
                + ",\"has_issues\":true,\"open_issues_count\":4}");
        }

        return "[" + string.Join(",", entries) + "]";
    }

    //A full page of repositories, so a test can prove the listing asks for the next one.
    internal static string ManyRepositories(string owner, int count, int firstIndex, bool archived = false)
    {
        var names = new List<string>(count);
        for (var index = 0; index < count; index++)
        {
            names.Add(owner + "/repo-" + (firstIndex + index).ToString("000", CultureInfo.InvariantCulture));
        }

        return Repositories(names.ToArray(), archived);
    }

    internal static Dictionary<string, string> RateLimitHeaders(int limit, int remaining,
        DateTimeOffset resetAt, string resource = "search")
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["x-ratelimit-limit"] = limit.ToString(CultureInfo.InvariantCulture),
            ["x-ratelimit-remaining"] = remaining.ToString(CultureInfo.InvariantCulture),
            ["x-ratelimit-reset"] = resetAt.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
            ["x-ratelimit-resource"] = resource,
        };
    }
}
