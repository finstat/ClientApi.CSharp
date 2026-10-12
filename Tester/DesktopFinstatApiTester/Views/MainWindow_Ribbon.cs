using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Birko.Xaml.Core.Commands;
using Birko.Xaml.Core.Ribbon;

namespace DesktopFinstatApiTester.Views
{
    /// <summary>
    /// The ribbon as data. Birko's <c>Ribbon</c> control renders <see cref="RibbonTab"/> /
    /// <see cref="RibbonGroup"/> / <see cref="RibbonItem"/>, so the whole command surface is this one
    /// table instead of a Fluent ribbon's XAML plus a click handler per button. The same table also
    /// feeds the Ctrl+K command palette, so every command is reachable by typing its name.
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>Glyphs kept in step with the web tester's <c>ICON</c> map in
        /// <c>src/shell/ribbon-tabs.ts</c>, so both testers label the same action the same way.</summary>
        private static class Glyph
        {
            public const string Detail = "🏢";
            public const string Monitoring = "👁";
            public const string Reporting = "📊";
            public const string Statement = "📑";
            public const string Diff = "Δ";
            public const string Distraint = "⚖";
            public const string Bankruptcy = "⚠";

            public const string List = "📋";
            public const string Add = "➕";
            public const string Remove = "➖";
            public const string Report = "📄";
            public const string Proceedings = "⚖";
            public const string Categories = "📗";
            public const string Search = "🔍";
            public const string Download = "⬇";
            public const string Open = "📂";
            public const string Legend = "🔑";
            public const string Person = "👤";
            public const string Company = "🏢";
            public const string Tier = "🔢";
            public const string Login = "🔑";
            public const string Date = "📅";
        }

        private void BuildRibbon()
        {
            var tabs = ApplyGroupDefaults(BuildRibbonTabs());
            ribbon.Tabs = tabs;

            // Every ribbon command, flattened into the palette, grouped by "Tab · Group".
            commandPalette.Commands = tabs
                .SelectMany(tab => tab.Groups.SelectMany(group => group.Items.Select(item => new CommandItem
                {
                    Id = item.Id,
                    Label = string.Format("{0}: {1}", group.Label, item.Label),
                    Group = tab.Label,
                    Run = item.Run,
                })))
                .ToList();
        }

        /// <summary>Glyph per tab, as the web tester's tab icons; a collapsed group shows its tab's glyph.</summary>
        private static readonly Dictionary<string, string> TabGlyphs = new Dictionary<string, string>
        {
            ["detail"] = Glyph.Detail,
            ["monitoring"] = Glyph.Monitoring,
            ["reporting"] = Glyph.Reporting,
            ["statement"] = Glyph.Statement,
            ["diff"] = Glyph.Diff,
            ["distraint"] = Glyph.Distraint,
            ["bankruptcy"] = Glyph.Bankruptcy,
        };

        /// <summary>
        /// Group settings that follow from the table's layout rather than being repeated per group:
        /// <list type="bullet">
        ///   <item>Groups are ordered most important first, so the leftmost gets the highest
        ///         <see cref="RibbonGroup.ScalingPriority"/> and a narrow window collapses groups from the right,
        ///         as Office does. At the default 0, ties degrade left to right and the primary Detail group was
        ///         the first to drop to identical icon-only buttons.</item>
        ///   <item>A group without its own <see cref="RibbonGroup.Icon"/> takes its tab's glyph, so a group folded
        ///         into a single button shows something meaningful instead of the generic ▦.</item>
        /// </list>
        /// </summary>
        private static IReadOnlyList<RibbonTab> ApplyGroupDefaults(IReadOnlyList<RibbonTab> tabs) =>
            tabs.Select(tab => new RibbonTab
            {
                Id = tab.Id,
                Label = tab.Label,
                Groups = tab.Groups.Select((group, index) => new RibbonGroup
                {
                    Label = group.Label,
                    Items = group.Items,
                    Icon = group.Icon ?? (TabGlyphs.TryGetValue(tab.Id, out var glyph) ? glyph : null),
                    MinSize = group.MinSize,
                    ScalingPriority = tab.Groups.Count - index,
                }).ToList(),
            }).ToList();

        /// <summary>Wrap an API call as a ribbon item. <see cref="RibbonItem.Run"/> is a plain
        /// <see cref="Action"/>, so the async request is started and its failure surfaced through the
        /// same exception dialog every other path uses (nothing is left unobserved).</summary>
        private RibbonItem Item(string id, string label, string icon, Func<Task> run) =>
            new RibbonItem
            {
                Id = id,
                Label = label,
                Icon = icon,
                Run = () => _ = FireAsync(run),
            };

        private async Task FireAsync(Func<Task> run)
        {
            try
            {
                await run();
            }
            catch (Exception ex)
            {
                await ShowExceptionAsync(ex);
            }
        }

        private IReadOnlyList<RibbonTab> BuildRibbonTabs() => new[]
        {
            new RibbonTab
            {
                Id = "detail",
                Label = "Detail",
                Groups = new[]
                {
                    new RibbonGroup
                    {
                        Label = "SK Detail",
                        Icon = Glyph.Company,
                        Items = new[]
                        {
                            Item("sk-detail-basic",    "Basic",    Glyph.Tier, SKBasicRequest),
                            Item("sk-detail-detail",   "Detail",   Glyph.Tier, SKDetailRequest),
                            Item("sk-detail-extended", "Extended", Glyph.Tier, SKExtendedRequest),
                            Item("sk-detail-ultimate", "Ultimate", Glyph.Tier, SKUltimateRequest),
                        },
                    },
                    new RibbonGroup
                    {
                        Label = "SK Other",
                        Icon = Glyph.Search,
                        Items = new[]
                        {
                            Item("sk-autocomplete", "Auto Complete", Glyph.Search, SKAutoCompleteRequest),
                            Item("sk-autologin",    "Auto LogIn",    Glyph.Login,  SKAutoLoginRequest),
                        },
                    },
                    new RibbonGroup
                    {
                        Label = "CZ Detail",
                        Icon = Glyph.Company,
                        Items = new[]
                        {
                            Item("cz-detail-basic",   "Basic",   Glyph.Tier, CZBasicRequest),
                            Item("cz-detail-detail",  "Detail",  Glyph.Tier, CZDetailRequest),
                            Item("cz-detail-premium", "Premium", Glyph.Tier, CZPremiumRequest),
                            Item("cz-detail-elite",   "Elite",   Glyph.Tier, CZEliteRequest),
                            Item("cz-detail-ultimate", "Ultimate", Glyph.Tier, CZUltimateRequest),
                        },
                    },
                    new RibbonGroup
                    {
                        Label = "CZ Other",
                        Icon = Glyph.Search,
                        Items = new[]
                        {
                            Item("cz-autocomplete", "Auto Complete", Glyph.Search, CZAutoCompleteRequest),
                            Item("cz-autologin",    "Auto LogIn",    Glyph.Login,  CZAutoLoginRequest),
                        },
                    },
                },
            },

            new RibbonTab
            {
                Id = "monitoring",
                Label = "Monitoring",
                Groups = new[]
                {
                    new RibbonGroup
                    {
                        Label = "SK Categories",
                        Icon = Glyph.Categories,
                        Items = new[]
                        {
                            Item("sk-mon-categories", "Categories", Glyph.Categories, SKMonitoringCategoriesRequest),
                        },
                    },
                    new RibbonGroup
                    {
                        Label = "SK ICO",
                        Icon = Glyph.Monitoring,
                        Items = new[]
                        {
                            Item("sk-mon-ico-add",         "Add",         Glyph.Add,         SKMonitoringICOAddRequest),
                            Item("sk-mon-ico-remove",      "Remove",      Glyph.Remove,      SKMonitoringICORemoveRequest),
                            Item("sk-mon-ico-list",        "List",        Glyph.List,        SKMonitoringICOListRequest),
                            Item("sk-mon-ico-report",      "Report",      Glyph.Report,      SKMonitoringICOReportRequest),
                            Item("sk-mon-ico-proceedings", "Proceedings", Glyph.Proceedings, SKMonitoringICOProceedingsRequest),
                        },
                    },
                    new RibbonGroup
                    {
                        Label = "SK Date",
                        Icon = Glyph.Date,
                        Items = new[]
                        {
                            Item("sk-mon-date-add",         "Add",         Glyph.Add,         SKMonitoringDateAddRequest),
                            Item("sk-mon-date-remove",      "Remove",      Glyph.Remove,      SKMonitoringDateRemoveRequest),
                            Item("sk-mon-date-list",        "List",        Glyph.List,        SKMonitoringDateListRequest),
                            Item("sk-mon-date-report",      "Report",      Glyph.Report,      SKMonitoringDateReportRequest),
                            Item("sk-mon-date-proceedings", "Proceedings", Glyph.Proceedings, SKMonitoringDateProceedingsRequest),
                        },
                    },
                    new RibbonGroup
                    {
                        Label = "CZ Categories",
                        Icon = Glyph.Categories,
                        Items = new[]
                        {
                            Item("cz-mon-categories", "Categories", Glyph.Categories, CZMonitoringCategoriesRequest),
                        },
                    },
                    new RibbonGroup
                    {
                        Label = "CZ ICO",
                        Icon = Glyph.Monitoring,
                        Items = new[]
                        {
                            Item("cz-mon-ico-add",    "Add",    Glyph.Add,    CZMonitoringICOAddRequest),
                            Item("cz-mon-ico-remove", "Remove", Glyph.Remove, CZMonitoringICORemoveRequest),
                            Item("cz-mon-ico-list",   "List",   Glyph.List,   CZMonitoringICOListRequest),
                            Item("cz-mon-ico-report", "Report", Glyph.Report, CZMonitoringICOReportRequest),
                        },
                    },
                    new RibbonGroup
                    {
                        Label = "CZ Date",
                        Icon = Glyph.Date,
                        Items = new[]
                        {
                            Item("cz-mon-date-add",    "Add",    Glyph.Add,    CZMonitoringDateAddRequest),
                            Item("cz-mon-date-remove", "Remove", Glyph.Remove, CZMonitoringDateRemoveRequest),
                            Item("cz-mon-date-list",   "List",   Glyph.List,   CZMonitoringDateListRequest),
                            Item("cz-mon-date-report", "Report", Glyph.Report, CZMonitoringDateReportRequest),
                        },
                    },
                },
            },

            new RibbonTab
            {
                Id = "reporting",
                Label = "Reporting",
                Groups = new[]
                {
                    new RibbonGroup
                    {
                        Label = "Reporting",
                        Items = new[]
                        {
                            Item("sk-reporting-topics",   "Topics",   Glyph.Categories, SKReportingTopicsRequest),
                            Item("sk-reporting-list",     "List",     Glyph.List,       SKReportingListRequest),
                            Item("sk-reporting-download", "Download", Glyph.Download,   SKDownloadReportingOutputRequest),
                        },
                    },
                },
            },

            new RibbonTab
            {
                Id = "statement",
                Label = "Statement",
                Groups = new[]
                {
                    new RibbonGroup
                    {
                        Label = "Statement",
                        Items = new[]
                        {
                            Item("sk-statement-list",   "List",   Glyph.List,      SKStatementListRequest),
                            Item("sk-statement-detail", "Detail", Glyph.Statement, SKStatementDetailRequest),
                            Item("sk-statement-legend", "Legend", Glyph.Legend,    SKStatementTemplateLegendRequest),
                        },
                    },
                },
            },

            new RibbonTab
            {
                Id = "diff",
                Label = "Diff",
                Groups = new[]
                {
                    new RibbonGroup
                    {
                        Label = "Daily",
                        Icon = Glyph.Diff,
                        Items = new[]
                        {
                            Item("sk-diff-daily-list", "List", Glyph.List,     SKDailyDiffListRequest),
                            Item("sk-diff-daily-file", "File", Glyph.Download, SKDailyDiffFileRequest),
                            Item("sk-diff-daily-open", "Open", Glyph.Open,     OpenDailyDiffFileRequest),
                        },
                    },
                    new RibbonGroup
                    {
                        Label = "Ultimate",
                        Icon = Glyph.Tier,
                        Items = new[]
                        {
                            Item("sk-diff-ultimate-list", "List", Glyph.List,     SKDailyUltimateDiffListRequest),
                            Item("sk-diff-ultimate-file", "File", Glyph.Download, SKDailyUltimateDiffFileRequest),
                            Item("sk-diff-ultimate-open", "Open", Glyph.Open,     OpenDailyUltimateDiffFileRequest),
                        },
                    },
                    new RibbonGroup
                    {
                        Label = "Statement",
                        Icon = Glyph.Statement,
                        Items = new[]
                        {
                            Item("sk-diff-statement-list",   "List",   Glyph.List,     SKDailyStatementDiffListRequest),
                            Item("sk-diff-statement-file",   "File",   Glyph.Download, SKDailyStatementDiffFileRequest),
                            Item("sk-diff-statement-open",   "Open",   Glyph.Open,     OpenDailyStatementDiffFileRequest),
                            Item("sk-diff-statement-legend", "Legend", Glyph.Legend,   SKDailyStatementDiffLegendRequest),
                        },
                    },
                    new RibbonGroup
                    {
                        Label = "Statement v2014",
                        Icon = Glyph.Legend,
                        Items = new[]
                        {
                            Item("sk-diff-statement2014-list",   "List",   Glyph.List,     SKDailyStatement2014DiffListRequest),
                            Item("sk-diff-statement2014-file",   "File",   Glyph.Download, SKDailyStatement2014DiffFileRequest),
                            Item("sk-diff-statement2014-open",   "Open",   Glyph.Open,     OpenDailyStatement2014DiffFileRequest),
                            Item("sk-diff-statement2014-legend", "Legend", Glyph.Legend,   SKDailyStatementDiff2014LegendRequest),
                        },
                    },
                },
            },

            new RibbonTab
            {
                Id = "distraint",
                Label = "Distraint",
                Groups = new[]
                {
                    // The WPF ribbon left the second group unlabeled; "Results" matches the label
                    // the web tester already gives the same three endpoints (they read back
                    // already-paid-for results rather than billing the account).
                    new RibbonGroup
                    {
                        Label = "Paid",
                        Icon = Glyph.Distraint,
                        Items = new[]
                        {
                            Item("sk-distraint-search", "Search", Glyph.Search,    SKDistraintSearchRequest),
                            Item("sk-distraint-detail", "Detail", Glyph.Distraint, SKDistraintDetailRequest),
                        },
                    },
                    new RibbonGroup
                    {
                        Label = "Results",
                        Icon = Glyph.List,
                        Items = new[]
                        {
                            Item("sk-distraint-results",       "Results",         Glyph.List,      SKDistraintResultsRequest),
                            Item("sk-distraint-results-token", "Results By Token", Glyph.List,     SKDistraintResultsByTokenRequest),
                            Item("sk-distraint-stored-detail", "Stored Detail",   Glyph.Distraint, SKDistraintStoredDetailRequest),
                        },
                    },
                },
            },

            new RibbonTab
            {
                Id = "bankruptcy",
                Label = "Bankruptcy / Restructuring",
                Groups = new[]
                {
                    new RibbonGroup
                    {
                        Label = "Persons",
                        Icon = Glyph.Person,
                        Items = new[]
                        {
                            Item("sk-bar-person", "Person", Glyph.Person, SKPersonBankruptcyProceedingsRequest),
                        },
                    },
                    new RibbonGroup
                    {
                        Label = "ICO",
                        Icon = Glyph.Company,
                        Items = new[]
                        {
                            Item("sk-bar-company", "Company", Glyph.Company, SKCompanyBankruptcyRestructuringRequest),
                        },
                    },
                },
            },
        };
    }
}
