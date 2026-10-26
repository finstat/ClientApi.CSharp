using System;
using System.Collections.Generic;
using System.Linq;

namespace DesktopFinstatApiTester.ViewModel
{
    /// <summary>One call of a comparison: the profile it went to and what came back.</summary>
    public sealed class CompareCell
    {
        public Model.EnvProfile Profile { get; set; }
        public bool Ok { get; set; }
        /// <summary>Why the call failed, short enough for a table cell; null when it succeeded.</summary>
        public string Error { get; set; }
        /// <summary>The body as FinStat sent it, in the comparison's format.</summary>
        public string Body { get; set; }
        /// <summary>Against the baseline cell of the same run; null for the baseline and when a call failed.</summary>
        public BodyDiff Diff { get; set; }
    }

    /// <summary>The request for one IČO (or the recorded request as is), sent to the baseline and every compared profile.</summary>
    public sealed class CompareRun
    {
        /// <summary>The IČO sent; null when the request has no IČO parameter.</summary>
        public string Value { get; set; }
        /// <summary>Index 0 is the baseline; null while the run is pending.</summary>
        public List<CompareCell> Cells { get; set; }
    }

    /// <summary>
    /// One run of the comparison window, kept in the history so it can be opened again as it was left: its
    /// setup, its results and the diff that was shown. Mirrors the web tester's comparison session.
    /// </summary>
    public sealed class CompareSession
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        /// <summary>The compared request, kept here so the comparison survives removing its history row.</summary>
        public ResponseItem Request { get; set; }
        public Model.EnvProfile Base { get; set; }
        public List<Model.EnvProfile> Rows { get; set; } = new List<Model.EnvProfile>();
        public List<string> Values { get; set; } = new List<string>();
        public bool Json { get; set; }
        public int Concurrency { get; set; } = 2;
        public bool HideHosts { get; set; } = true;
        /// <summary>The window's own list: calls still running fill it in after the session is saved.</summary>
        public List<CompareRun> Runs { get; set; } = new List<CompareRun>();
        public int SelectedRun { get; set; } = -1;
        public int SelectedColumn { get; set; } = -1;

        public IEnumerable<Model.EnvProfile> Profiles => new[] { Base }.Concat(Rows);
    }
}
