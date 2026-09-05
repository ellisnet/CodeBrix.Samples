using System;
using System.Collections.Generic;

namespace GitHubIssueFinder.GitHub.Tests;

//Keeps every progress report a search made, so a test can read the sequence of phases.
internal sealed class RecordingProgress : IProgress<SearchProgress>
{
    private readonly List<SearchProgress> _reports = new List<SearchProgress>();

    public void Report(SearchProgress value)
    {
        lock (_reports) { _reports.Add(value); }
    }

    internal IReadOnlyList<SearchProgress> Reports
    {
        get { lock (_reports) { return _reports.ToArray(); } }
    }

    //The phases in order, with a repeated phase collapsed to one entry, which is the shape
    //a test wants to read.
    internal IReadOnlyList<SearchPhase> PhaseSequence()
    {
        var sequence = new List<SearchPhase>();
        foreach (var report in Reports)
        {
            if (sequence.Count > 0 && sequence[sequence.Count - 1] == report.Phase) { continue; }
            sequence.Add(report.Phase);
        }

        return sequence;
    }

    internal IReadOnlyList<SearchProgress> Of(SearchPhase phase)
    {
        var matching = new List<SearchProgress>();
        foreach (var report in Reports)
        {
            if (report.Phase == phase) { matching.Add(report); }
        }

        return matching;
    }
}
