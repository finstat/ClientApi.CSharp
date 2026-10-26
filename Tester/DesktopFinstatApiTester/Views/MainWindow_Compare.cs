using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;
using DesktopFinstatApiTester.ViewModel;

namespace DesktopFinstatApiTester.Views
{
    /// <summary>
    /// Comparisons in the history: a comparison is a row of its own, its calls listed under it, and opening the row
    /// brings back its window as the run was left. Mirrors the web tester's grouped history.
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>Open comparison windows by session, so opening a comparison twice brings its window forward.</summary>
        private readonly Dictionary<string, CompareWindow> _compareWindows = new Dictionary<string, CompareWindow>();

        /// <summary>Requests whose first parameter is an IČO; a comparison can swap it for a list of IČOs.</summary>
        private static readonly HashSet<string> IcoRequests = new HashSet<string>
        {
            "Basic", "Detail", "Extended", "Ultimate", "PremiumCZ", "EliteCZ", "UltimateCZ",
            "CompanyBankruptcyRestructuring", "DistraintSearch", "DistraintResults",
            "MonitoringICOAdd", "MonitoringICORemove", "StatementList", "StatementDetail",
        };

        /// <summary>Requests that save a file instead of returning a body; there is nothing to compare.</summary>
        private static readonly HashSet<string> FileRequests = new HashSet<string>
        {
            "DailyDiffFile", "DailyUltimateDiffFile", "DailyStatementDiffFile", "DailyStatement2014DiffFile", "DownloadReportingOutput",
        };

        internal static int IcoParameterIndex(ResponseItem item) => IcoRequests.Contains(item.Request) ? 0 : -1;

        internal Model.Settings CurrentSettings => AppInstance.Settings;

        internal Model.EnvProfile ProfileOfRow(ResponseItem item) => ProfileOfItem(item);

        private void dataGridCompare_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) =>
            _ = FireAsync(() => OpenComparisonAsync(datagridResponse.SelectedItem as ResponseItem));

        private void InitializeComparisons()
        {
            datagridResponse.DoubleTapped += (_, _) =>
            {
                if (datagridResponse.SelectedItem is ResponseItem { Compare: not null } item)
                {
                    _ = FireAsync(() => OpenComparisonAsync(item));
                }
            };
        }

        /// <summary>A comparison's row opens that comparison; any other row starts a new one from its request.</summary>
        private async System.Threading.Tasks.Task OpenComparisonAsync(ResponseItem item)
        {
            if (item == null)
            {
                await _dialogs.AlertAsync("Select a request in the history first", "Attention");
                return;
            }
            if (item.Compare != null)
            {
                if (_compareWindows.TryGetValue(item.Compare.Id, out var open))
                {
                    open.Activate();
                    return;
                }
                Track(new CompareWindow(this, null, item.Compare)).Show(this);
                return;
            }
            if (FileRequests.Contains(item.Request) || ApiCallFor(item) == null)
            {
                await _dialogs.AlertAsync("This request cannot be compared: it saves a file or calls no API.", "Attention");
                return;
            }
            Track(new CompareWindow(this, item, null)).Show(this);
        }

        private CompareWindow Track(CompareWindow window)
        {
            if (window.Session != null) _compareWindows[window.Session.Id] = window;
            window.Closed += (_, _) =>
            {
                foreach (var key in _compareWindows.Where(kv => kv.Value == window).Select(kv => kv.Key).ToList())
                {
                    _compareWindows.Remove(key);
                }
            };
            return window;
        }

        /// <summary>Saves a comparison that is about to run as its own history row; the window that runs it is the one it opens.</summary>
        internal ResponseItem RecordComparison(CompareSession session, CompareWindow window)
        {
            foreach (var key in _compareWindows.Where(kv => kv.Value == window).Select(kv => kv.Key).ToList())
            {
                _compareWindows.Remove(key);
            }
            _compareWindows[session.Id] = window;

            var row = new ResponseItem("⇄ Compare: " + session.Request.Request, session.Request.ApiSource,
                session.Values.Count > 0 ? session.Values.Cast<object>().ToList() : session.Request.Parameters)
            {
                Compare = session,
                Profile = string.Join(" vs ", session.Profiles.Select(p => p.Label)),
            };
            AppInstance.Add(row);
            datagridResponse.SelectedItem = row;
            return row;
        }

        /// <summary>Records one call of a single-IČO comparison under the comparison's row.</summary>
        internal void RecordComparisonCall(ResponseItem parent, ResponseItem request, object[] args, Model.EnvProfile profile, ComparisonCall call)
        {
            if (parent == null || call == null)
            {
                return;
            }
            var item = new ResponseItem(request.Request, request.ApiSource, args)
            {
                Profile = profile.Label,
                Content = call.Call.Content,
                RequestHeaders = call.Call.RequestHeaders,
                ResponseHeaders = call.Call.ResponseHeaders,
            };
            item.AddData(call.Error == null ? new[] { call.Result } : Array.Empty<object>());
            AppInstance.AddToGroup(parent, item);
        }
    }
}
