extern alias CZ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using DesktopFinstatApiTester.Model;
using DesktopFinstatApiTester.ViewModel;

namespace DesktopFinstatApiTester.Views
{
    /// <summary>
    /// The comparison of one recorded request across environment profiles. Each Run is saved as a comparison in the
    /// history (its own row, with its calls under it when one IČO was compared); opening that row shows the run again
    /// as it was left, without calling anything.
    /// </summary>
    public partial class CompareWindow : Window
    {
        private static readonly int[] ConcurrencyOptions = { 1, 2, 4, 8 };

        private readonly MainWindow _main;
        private readonly ResponseItem _request;
        private readonly int _icoIndex;
        /// <summary>The run shown; null until the first Run of a new comparison.</summary>
        private CompareSession _session;
        private ResponseItem _historyRow;
        private CancellationTokenSource _cancel;

        /// <summary>Parameterless ctor so the AXAML stays loadable by the runtime XAML loader (AVLN3001).</summary>
        public CompareWindow() : this(null, null, null) { }

        public CompareWindow(MainWindow main, ResponseItem request, CompareSession saved)
        {
            InitializeComponent();
            _main = main;
            _request = saved?.Request ?? request;
            _session = saved;
            if (_main == null || _request == null)
            {
                return;
            }
            _icoIndex = MainWindow.IcoParameterIndex(_request);
            Title = string.Format("Compare: {0} ({1})", _request.Request, _request.ApiSource);
            textBlockTitle.Text = Title;

            var settings = _main.CurrentSettings;
            FillSide(panelBase, saved?.Base ?? ProfileOf(_request), removable: false);
            foreach (var profile in saved?.Rows ?? new List<EnvProfile> { NextProfile() })
            {
                AddRow(profile);
            }

            comboBoxFormat.ItemsSource = new[] { "XML", "JSON" };
            comboBoxFormat.SelectedIndex = (saved?.Json ?? settings.ResponseType == ResponseType.JSON) ? 1 : 0;
            comboBoxConcurrency.ItemsSource = ConcurrencyOptions;
            comboBoxConcurrency.SelectedItem = saved?.Concurrency ?? 2;
            panelValues.IsVisible = panelConcurrency.IsVisible = _icoIndex >= 0;
            if (_icoIndex >= 0)
            {
                textBoxValues.Text = saved != null
                    ? string.Join(Environment.NewLine, saved.Values)
                    : Convert.ToString(_request.Parameters?.ElementAtOrDefault(_icoIndex));
            }
            checkBoxHideHosts.IsChecked = saved?.HideHosts ?? true;
            UpdateRunLabel();

            if (saved != null)
            {
                expanderSetup.IsExpanded = false;
                var done = saved.Runs.Count(r => r.Cells != null);
                ShowProgress(done, saved.Runs.Count, running: false);
                RenderResults();
                RenderFields();
                if (Cell(saved.SelectedRun, saved.SelectedColumn)?.Diff != null) Show(saved.SelectedRun, saved.SelectedColumn);
                else ShowFirst();
            }
        }

        public CompareSession Session => _session;

        protected override void OnClosed(EventArgs e)
        {
            _cancel?.Cancel();
            base.OnClosed(e);
        }

        #region Setup

        /// <summary>History records the profile by name (plus ":port" when one was set).</summary>
        private EnvProfile ProfileOf(ResponseItem item) => _main.ProfileOfRow(item);

        private IReadOnlyList<EnvProfile> Profiles => _main.CurrentSettings.Profiles;

        /// <summary>The first profile not picked on any side yet; the active one when every profile is in use.</summary>
        private EnvProfile NextProfile()
        {
            var used = new HashSet<string>(Sides().Select(s => ReadSide(s).Id));
            return Profiles.FirstOrDefault(p => !used.Contains(p.Id)) ?? _main.CurrentSettings.ActiveProfile;
        }

        private IEnumerable<StackPanel> Sides() =>
            new[] { panelBase }.Concat(panelRows.Children.OfType<StackPanel>());

        private void FillSide(StackPanel side, EnvProfile profile, bool removable)
        {
            side.Children.Clear();
            var select = new ComboBox { Width = 180, ItemsSource = Profiles.Select(p => p.Name).ToList() };
            var port = new TextBox { Width = 90, Watermark = "443" };
            ToolTip.SetTip(port, "Port for this comparison only; empty = 443.");
            var index = Profiles.ToList().FindIndex(p => p.Id == profile.Id);
            select.SelectedIndex = Math.Max(0, index);
            port.Text = profile.Port?.ToString();
            // a picked profile brings its own port; the port can still be changed for this comparison
            select.SelectionChanged += (_, _) =>
            {
                if (select.SelectedIndex >= 0) port.Text = Profiles[select.SelectedIndex].Port?.ToString();
            };
            side.Children.Add(select);
            side.Children.Add(port);
            if (removable)
            {
                var remove = new Button { Content = "Remove" };
                remove.Click += (_, _) => { panelRows.Children.Remove(side); UpdateRunLabel(); };
                side.Children.Add(remove);
            }
        }

        private void AddRow(EnvProfile profile)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
            panelRows.Children.Add(row);
            FillSide(row, profile, removable: true);
        }

        /// <summary>The profile picked on one side, with the port typed next to it (only for this comparison).</summary>
        private EnvProfile ReadSide(StackPanel side)
        {
            var select = side.Children.OfType<ComboBox>().First();
            var portText = side.Children.OfType<TextBox>().First().Text?.Trim();
            var profile = Profiles[Math.Max(0, select.SelectedIndex)].Clone();
            profile.Port = int.TryParse(portText, out var port) && port >= 1 && port <= 65535 ? port : (int?)null;
            return profile;
        }

        /// <summary>IČOs typed in the box, in order, without repeats; the recorded request as is when there are none.</summary>
        private List<string> Values()
        {
            if (_icoIndex < 0)
            {
                return new List<string> { null };
            }
            var values = Regex.Split(textBoxValues.Text ?? string.Empty, @"[\s,;]+")
                .Where(v => v.Length > 0).Distinct().ToList();
            return values.Count > 0 ? values : new List<string> { null };
        }

        /// <summary>The button says how many calls a comparison makes: each IČO goes to every profile.</summary>
        private void UpdateRunLabel()
        {
            var calls = Values().Count * (1 + panelRows.Children.Count);
            buttonRun.Content = string.Format("Run ({0} call{1})", calls, calls == 1 ? string.Empty : "s");
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            AddRow(NextProfile());
            UpdateRunLabel();
        }

        private void textBoxValues_TextChanged(object sender, TextChangedEventArgs e) => UpdateRunLabel();

        private void checkBoxHideHosts_Changed(object sender, RoutedEventArgs e)
        {
            if (_session == null)
            {
                return;
            }
            _session.HideHosts = checkBoxHideHosts.IsChecked == true;
            foreach (var run in _session.Runs) DiffRun(run);
            RenderResults();
            RenderFields();
            if (Cell(_session.SelectedRun, _session.SelectedColumn)?.Diff != null) Show(_session.SelectedRun, _session.SelectedColumn);
            else ShowFirst();
        }

        #endregion

        #region Run

        private async void buttonRun_Click(object sender, RoutedEventArgs e)
        {
            if (_cancel != null)
            {
                return;
            }
            var profiles = Sides().Select(ReadSide).ToList();
            var values = Values();
            // one IČO is a single comparison and its calls go under it in the history; a list would flood it
            var record = values.Count == 1;
            var session = new CompareSession
            {
                Request = _request,
                Base = profiles[0],
                Rows = profiles.Skip(1).ToList(),
                Values = values.Where(v => v != null).ToList(),
                Json = comboBoxFormat.SelectedIndex == 1,
                Concurrency = comboBoxConcurrency.SelectedItem is int n ? n : 2,
                HideHosts = checkBoxHideHosts.IsChecked == true,
                Runs = values.Select(v => new CompareRun { Value = v }).ToList(),
            };
            _session = session;
            _historyRow = _main.RecordComparison(session, this);

            expanderSetup.IsExpanded = false;
            tabControlDetail.IsVisible = false;
            RenderResults();
            RenderFields();
            ShowProgress(0, values.Count, running: true);

            _cancel = new CancellationTokenSource();
            var token = _cancel.Token;
            var queue = new Queue<int>(Enumerable.Range(0, values.Count));
            var done = 0;

            // continuations come back to the UI thread, so the queue and the session are only touched there
            async Task Worker()
            {
                while (!token.IsCancellationRequested && queue.Count > 0)
                {
                    var i = queue.Dequeue();
                    var args = ArgsFor(values[i]);
                    var calls = await Task.WhenAll(profiles.Select(p => _main.CallForComparisonAsync(_request, args, p, session.Json)));
                    session.Runs[i].Cells = profiles.Select((p, c) => ToCell(p, calls[c])).ToList();
                    DiffRun(session.Runs[i]);
                    if (record)
                    {
                        for (int c = 0; c < profiles.Count; c++) _main.RecordComparisonCall(_historyRow, _request, args, profiles[c], calls[c]);
                    }
                    done++;
                    ShowProgress(done, values.Count, running: true);
                    RenderResults();
                    if (session.SelectedRun < 0) ShowFirst();
                }
            }

            await Task.WhenAll(Enumerable.Range(0, Math.Min(session.Concurrency, values.Count)).Select(_ => Worker()));
            ShowProgress(done, values.Count, running: false);
            RenderFields();
            _cancel = null;
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e) => _cancel?.Cancel();

        /// <summary>The recorded parameters, with the IČO swapped for <paramref name="value"/>.</summary>
        private object[] ArgsFor(string value)
        {
            var args = (_request.Parameters ?? Enumerable.Empty<object>()).ToArray();
            if (value != null && _icoIndex >= 0 && _icoIndex < args.Length)
            {
                args[_icoIndex] = value;
            }
            return args;
        }

        private static CompareCell ToCell(EnvProfile profile, MainWindow.ComparisonCall call)
        {
            if (call == null)
            {
                return new CompareCell { Profile = profile, Error = "This request cannot be repeated." };
            }
            return new CompareCell
            {
                Profile = profile,
                Ok = call.Error == null,
                Error = call.Error == null ? null : CallError(call.Error),
                Body = call.Body,
            };
        }

        /// <summary>
        /// The error of a failed call, short enough for a table cell. When the server answered with a web page instead
        /// of the API (FinStat quotes it after "Server response:"), the page is replaced by a note: the endpoint usually
        /// does not exist on that profile yet.
        /// </summary>
        private static string CallError(Exception ex)
        {
            string failType = null;
            if (ex is FinstatApi.FinstatApiException sk) failType = sk.FailType.ToString();
            else if (ex is CZ::FinstatApi.FinstatApiException cz) failType = cz.FailType.ToString();
            var error = ex.Message ?? ex.GetType().Name;
            var at = error.IndexOf("Server response:", StringComparison.Ordinal);
            if (at >= 0)
            {
                var quoted = error.Substring(at + "Server response:".Length);
                var message = error.Substring(0, at).Trim();
                error = Regex.IsMatch(quoted, "<(!doctype|html)", RegexOptions.IgnoreCase)
                    ? message + " (the server answered with a web page: the endpoint may not exist on this profile)"
                    : message;
            }
            if (error.Length > 300) error = error.Substring(0, 300) + "…";
            return failType != null ? string.Format("{0} — {1}", failType, error) : error;
        }

        /// <summary>Diff every compared profile of one run against its baseline. A failed call is not compared.</summary>
        private void DiffRun(CompareRun run)
        {
            if (run.Cells == null)
            {
                return;
            }
            var baseline = run.Cells[0];
            foreach (var other in run.Cells.Skip(1))
            {
                other.Diff = baseline.Ok && other.Ok
                    ? BodyDiffer.Diff(baseline.Body, other.Body, !_session.Json, _session.HideHosts ? HostNormalizer(baseline.Profile, other.Profile) : null)
                    : null;
            }
        }

        /// <summary>
        /// Replaces the hosts of every profile (with or without www., with any port) by {sk} / {cz}: a dev server links
        /// to the production domain, so the hosts of the two compared profiles alone are not enough. Longest first, so
        /// cz.finstat.sk is not half-replaced as cz.{sk}.
        /// </summary>
        private Func<string, string> HostNormalizer(EnvProfile a, EnvProfile b)
        {
            var hosts = new Dictionary<string, string>();
            foreach (var p in Profiles.Concat(new[] { a, b }))
            {
                hosts[BareHost(p.SkHost)] = "{sk}";
                hosts[BareHost(p.CzHost)] = "{cz}";
            }
            var names = hosts.Keys.OrderByDescending(h => h.Length).Select(Regex.Escape);
            var pattern = new Regex(string.Format(@"(?:www\.)?({0})(?::\d+)?", string.Join("|", names)), RegexOptions.IgnoreCase);
            return value => pattern.Replace(value, m => hosts.TryGetValue(m.Groups[1].Value.ToLowerInvariant(), out var h) ? h : m.Value);
        }

        private static string BareHost(string host)
        {
            var h = (host ?? string.Empty).ToLowerInvariant();
            return h.StartsWith("www.", StringComparison.Ordinal) ? h.Substring(4) : h;
        }

        #endregion

        #region Results

        private void ShowProgress(int done, int total, bool running)
        {
            panelProgress.IsVisible = total > 1 || running;
            progressBar.Maximum = Math.Max(1, total);
            progressBar.Value = done;
            textBlockProgress.Text = string.Format("{0} of {1} IČOs compared", done, total);
            buttonStop.IsVisible = running;
            buttonRun.IsEnabled = !running;
        }

        private CompareCell Cell(int run, int col)
        {
            var cells = run >= 0 && _session != null && run < _session.Runs.Count ? _session.Runs[run].Cells : null;
            return cells != null && col >= 0 && col < cells.Count ? cells[col] : null;
        }

        private static string Label(EnvProfile p) => p.Port.HasValue ? string.Format("{0}:{1}", p.Name, p.Port) : p.Name;

        /// <summary>One row per IČO, one column per profile; a compared cell shows its result and opens its diff.</summary>
        private void RenderResults()
        {
            gridResults.Children.Clear();
            gridResults.RowDefinitions.Clear();
            gridResults.ColumnDefinitions.Clear();
            if (_session == null)
            {
                return;
            }
            var profiles = _session.Profiles.ToList();
            var showValue = _session.Runs.Any(r => r.Value != null);
            var offset = showValue ? 1 : 0;
            for (int c = 0; c < profiles.Count + offset; c++) gridResults.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            for (int r = 0; r <= _session.Runs.Count; r++) gridResults.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            if (showValue) Put(Text("IČO", bold: true), 0, 0);
            for (int c = 0; c < profiles.Count; c++)
            {
                Put(Text(Label(profiles[c]) + (c == 0 ? " (baseline)" : string.Empty), bold: true), 0, c + offset);
            }
            for (int r = 0; r < _session.Runs.Count; r++)
            {
                var run = _session.Runs[r];
                if (showValue) Put(Text(run.Value ?? string.Empty), r + 1, 0);
                if (run.Cells == null)
                {
                    var pending = Text("…", muted: true);
                    Grid.SetColumnSpan(pending, profiles.Count);
                    Put(pending, r + 1, offset);
                    continue;
                }
                for (int c = 0; c < run.Cells.Count; c++) Put(CellControl(run.Cells[c], r, c), r + 1, c + offset);
            }
        }

        private Control CellControl(CompareCell cell, int run, int col)
        {
            if (!cell.Ok)
            {
                var fail = Text(cell.Error, brushKey: "BColorDangerTextBrush");
                fail.MaxWidth = 360;
                fail.TextWrapping = TextWrapping.Wrap;
                return fail;
            }
            if (col == 0) return Text("OK", brushKey: "BColorSuccessTextBrush");
            if (cell.Diff == null) return Text("Not compared, a call failed", muted: true);
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
            panel.Children.Add(cell.Diff.Equal
                ? Text("Equal", muted: true)
                : Text(string.Format("+{0} −{1} ~{2}", cell.Diff.Added, cell.Diff.Removed, cell.Diff.Changed)));
            var selected = _session.SelectedRun == run && _session.SelectedColumn == col;
            var button = new Button { Content = selected ? "Shown" : "Show diff", IsEnabled = !selected, Padding = new Avalonia.Thickness(8, 2) };
            button.Click += (_, _) => { Show(run, col); tabControlDetail.SelectedIndex = 0; };
            panel.Children.Add(button);
            return panel;
        }

        private TextBlock Text(string text, bool bold = false, bool muted = false, string brushKey = null)
        {
            var block = new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center };
            if (bold) block.FontWeight = FontWeight.SemiBold;
            var key = brushKey ?? (muted ? "BTextMutedBrush" : null);
            if (key != null) block.Bind(TextBlock.ForegroundProperty, block.GetResourceObservable(key));
            return block;
        }

        private void Put(Control control, int row, int col)
        {
            control.Margin = new Avalonia.Thickness(0, 2, 16, 2);
            Grid.SetRow(control, row);
            Grid.SetColumn(control, col);
            gridResults.Children.Add(control);
        }

        /// <summary>The first compared cell that differs, else the first compared one.</summary>
        private void ShowFirst()
        {
            (int, int)? fallback = null;
            for (int r = 0; r < _session.Runs.Count; r++)
            {
                var cells = _session.Runs[r].Cells ?? new List<CompareCell>();
                for (int c = 1; c < cells.Count; c++)
                {
                    if (cells[c].Diff == null) continue;
                    if (!cells[c].Diff.Equal) { Show(r, c); return; }
                    fallback ??= (r, c);
                }
            }
            if (fallback.HasValue) Show(fallback.Value.Item1, fallback.Value.Item2);
            else Show(-1, -1);
        }

        /// <summary>Show the diff of one cell against the baseline of its run; -1 hides it.</summary>
        private void Show(int run, int col)
        {
            _session.SelectedRun = run;
            _session.SelectedColumn = col;
            var baseline = Cell(run, 0);
            var other = Cell(run, col);
            tabControlDetail.IsVisible = other?.Diff != null || tabItemFields.IsVisible;
            if (baseline == null || other?.Diff == null)
            {
                textBlockDiffTitle.Text = string.Empty;
                diffViewer.SetData(null, null);
                buttonBodyA.IsVisible = buttonBodyB.IsVisible = false;
                RenderResults();
                return;
            }
            var value = _session.Runs[run].Value;
            textBlockDiffTitle.Text = string.Format("{0}{1} vs {2}", value != null ? value + ": " : string.Empty, Label(baseline.Profile), Label(other.Profile));
            buttonBodyA.IsVisible = buttonBodyB.IsVisible = true;
            diffViewer.LabelA = Label(baseline.Profile);
            diffViewer.LabelB = Label(other.Profile);
            diffViewer.Format = _session.Json ? Birko.Xaml.Avalonia.Controls.JsonDiffFormat.Json : Birko.Xaml.Avalonia.Controls.JsonDiffFormat.Xml;
            // the same options the results table counted with, so the viewer agrees with it
            diffViewer.Options = BodyDiffer.Options(_session.HideHosts ? HostNormalizer(baseline.Profile, other.Profile) : null);
            diffViewer.SetData(baseline.Body, other.Body);
            RenderResults();
        }

        private void buttonBodyA_Click(object sender, RoutedEventArgs e) => ShowBody(0);

        private void buttonBodyB_Click(object sender, RoutedEventArgs e) => ShowBody(_session?.SelectedColumn ?? -1);

        private void ShowBody(int col)
        {
            var cell = Cell(_session?.SelectedRun ?? -1, col);
            if (cell?.Body != null)
            {
                new OutputWindow(cell.Body) { Title = string.Format("Response body — {0}", Label(cell.Profile)) }.Show(this);
            }
        }

        /// <summary>
        /// Which fields differ, and in how many IČOs, per compared profile. List positions are folded into [*] so "the
        /// third contact differs" and "the fifth contact differs" count as one field.
        /// </summary>
        private void RenderFields()
        {
            var runs = _session?.Runs.Where(r => r.Cells != null).ToList() ?? new List<CompareRun>();
            var counts = new Dictionary<(string Profile, string Path), int>();
            if (runs.Count >= 2)
            {
                foreach (var cell in runs.SelectMany(r => r.Cells.Skip(1)).Where(c => c.Diff != null))
                {
                    foreach (var path in cell.Diff.Changes.Select(ch => Regex.Replace(ch.Path, @"\[\d+\]", "[*]")).Distinct())
                    {
                        var key = (Label(cell.Profile), path);
                        counts[key] = counts.TryGetValue(key, out var n) ? n + 1 : 1;
                    }
                }
            }
            tabItemFields.IsVisible = counts.Count > 0;
            tabItemFields.Header = string.Format("Fields that differ ({0})", counts.Count);
            var text = new StringBuilder();
            text.AppendFormat("Fields that differ in at least one of {0} IČOs (top 50)", runs.Count).AppendLine().AppendLine();
            foreach (var entry in counts.OrderByDescending(e => e.Value).ThenBy(e => e.Key.Path).Take(50))
            {
                text.AppendFormat("{0,-24} {1,-60} {2}", entry.Key.Profile, entry.Key.Path.Length == 0 ? "/" : entry.Key.Path, entry.Value).AppendLine();
            }
            textBoxFields.Text = text.ToString();
            if (counts.Count > 0) tabControlDetail.IsVisible = true;
        }

        #endregion
    }
}
