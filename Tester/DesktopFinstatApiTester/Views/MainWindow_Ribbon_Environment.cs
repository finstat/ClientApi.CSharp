using System;
using System.Collections.Generic;
using System.Linq;
using Birko.Xaml.Avalonia.Controls;
using Birko.Xaml.Core.Ribbon;
using DesktopFinstatApiTester.Model;

namespace DesktopFinstatApiTester.Views
{
    /// <summary>
    /// The ribbon around the endpoint tabs, kept in step with the web tester's <c>ribbon-tabs.ts</c>: the
    /// Environment tab (port of the active profile, response type, language; its launcher opens Settings),
    /// the contextual Response tab shown while a history row is selected, the endpoint tooltips and the
    /// Quick Access Toolbar.
    /// </summary>
    public partial class MainWindow
    {
        private const string EnvironmentTabId = "environment";
        private const string ResponseTabId = "response";
        /// <summary>Contextual set of the Response tab.</summary>
        private const string ResponseSet = "response";

        private static class EnvGlyph
        {
            public const string Environment = "⚙";
            public const string Response = "📄";
            public const string Rerun = "↻";
            public const string Curl = ">_";
            public const string Body = "📄";
            public const string Remove = "➖";
            public const string Clear = "✕";
        }

        /// <summary>The endpoint an item calls, on which host: "SK API on www.finstat.sk"; null for local commands (open a file).</summary>
        private string EndpointTooltip(string itemId)
        {
            var cz = itemId.StartsWith("cz-", StringComparison.Ordinal);
            if ((!cz && !itemId.StartsWith("sk-", StringComparison.Ordinal)) || itemId.EndsWith("-open", StringComparison.Ordinal))
            {
                return null;
            }
            var profile = AppInstance.Settings.ActiveProfile;
            var port = profile.Port.HasValue ? ":" + profile.Port : string.Empty;
            return string.Format("{0} API on {1}{2}", cz ? "CZ" : "SK", cz ? profile.CzHost : profile.SkHost, port);
        }

        private RibbonTab EnvironmentTab()
        {
            var settings = AppInstance.Settings;
            var profile = settings.ActiveProfile;
            const string responseTypeHint =
                "Wire format between the tester and FinStat; the Data tab shows the parsed result either way.";
            return new RibbonTab
            {
                Id = EnvironmentTabId,
                Label = "Environment",
                Groups = new[]
                {
                    new RibbonGroup
                    {
                        Id = "env-profile",
                        Label = "Profile",
                        Icon = EnvGlyph.Environment,
                        Launcher = true,
                        LauncherLabel = "Settings",
                        Items = new[]
                        {
                            new RibbonItem
                            {
                                Id = "env-port",
                                Label = "Port",
                                // the tab row shows which profile is active; the tooltip says whose port this is
                                Description = string.Format("{0}: {1}, {2}", profile.Name, profile.SkUrl, profile.CzUrl),
                                Field = new RibbonField
                                {
                                    Type = RibbonFieldType.Number,
                                    Value = profile.Port?.ToString() ?? string.Empty,
                                    Placeholder = "443",
                                    Min = 1,
                                    Max = 65535,
                                    Width = 6,
                                    Caption = RibbonFieldCaption.Above,
                                },
                            },
                        },
                    },
                    new RibbonGroup
                    {
                        Id = "env-response",
                        Label = "Response",
                        Icon = EnvGlyph.Response,
                        Items = new[]
                        {
                            new RibbonItem { Id = "rt-xml",  Label = "XML",  Toggle = true, ToggleGroup = "responseType", Pressed = settings.ResponseType == ResponseType.XML,  Description = responseTypeHint },
                            new RibbonItem { Id = "rt-json", Label = "JSON", Toggle = true, ToggleGroup = "responseType", Pressed = settings.ResponseType == ResponseType.JSON, Description = responseTypeHint },
                            new RibbonItem
                            {
                                Id = "env-lang",
                                Label = "Language",
                                Description = "Sent as the Lang parameter. Only finstat.cz honours it; finstat.sk always answers in Slovak. A CZ request can override it.",
                                Field = new RibbonField
                                {
                                    Type = RibbonFieldType.Select,
                                    Value = LangValue(settings.Lang),
                                    Options = new[]
                                    {
                                        new RibbonFieldOption { Value = string.Empty, Label = "Server default" },
                                        new RibbonFieldOption { Value = "cs", Label = "CS" },
                                        new RibbonFieldOption { Value = "sk", Label = "SK" },
                                    },
                                    Width = 12,
                                    Caption = RibbonFieldCaption.Above,
                                },
                            },
                        },
                    },
                },
            };
        }

        private static string LangValue(ResponseLang lang) =>
            lang == ResponseLang.Default ? string.Empty : lang.ToString().ToLowerInvariant();

        /// <summary>The selected history row's actions, as its context menu.</summary>
        private RibbonTab ResponseTab()
        {
            RibbonItem Action(string id, string label, string icon, Action run) =>
                new RibbonItem { Id = "resp-" + id, Label = label, Icon = icon, Run = run };
            return new RibbonTab
            {
                Id = ResponseTabId,
                Label = "Response",
                ContextualSet = ResponseSet,
                Groups = new[]
                {
                    new RibbonGroup
                    {
                        Id = "resp-call",
                        Label = "Call",
                        Icon = EnvGlyph.Rerun,
                        Items = new[]
                        {
                            Action("rerun", "Rerun",         EnvGlyph.Rerun, () => dataGridRepeatRequest_Click(null, null)),
                            Action("curl",  "curl",          EnvGlyph.Curl,  () => dataGridCurlCommand_Click(null, null)),
                            Action("body",  "Response body", EnvGlyph.Body,  () => dataGridShowResponseOutputWindow_Click(null, null)),
                        },
                    },
                    new RibbonGroup
                    {
                        Id = "resp-history",
                        Label = "History",
                        Icon = EnvGlyph.Remove,
                        Items = new[]
                        {
                            Action("remove", "Remove",    EnvGlyph.Remove, RemoveSelectedHistory),
                            Action("clear",  "Clear all", EnvGlyph.Clear,  ClearHistory),
                        },
                    },
                },
            };
        }

        private void RemoveSelectedHistory()
        {
            if (datagridResponse.SelectedItem is ViewModel.ResponseItem item)
            {
                AppInstance.ResponseItems.Remove(item);
            }
        }

        private void ClearHistory() => AppInstance.ResponseItems.Clear();

        /// <summary>The Response tab is shown while a history row is selected.</summary>
        private void SyncResponseTab()
        {
            if (datagridResponse.SelectedItem != null)
            {
                ribbon.ShowContextual(ResponseSet);
            }
            else
            {
                ribbon.HideContextual(ResponseSet);
            }
        }

        /// <summary>Events of the ribbon's own controls; wired once, the tabs are rebuilt when the profile changes.</summary>
        private void InitializeRibbonExtras()
        {
            ribbon.SetContextualSets(new[]
            {
                new RibbonContextualSet { Id = ResponseSet, Label = "Response", Color = RibbonContextualColor.Info },
            });
            ribbon.SetQuickAccess(new[]
            {
                new RibbonItem { Id = "settings", Label = "Settings", Icon = EnvGlyph.Environment, Run = () => buttonSettings_Click(null, null) },
            });
            // the user's toolbar, saved with the settings; null = the defaults above
            ribbon.SetQuickAccessRefs(AppInstance.Settings.QuickAccess);
            ribbon.QuickAccessChanged += (_, refs) =>
            {
                AppInstance.Settings.QuickAccess = refs.ToList();
                AppInstance.Save();
            };
            ribbon.GroupLauncherInvoked += (_, e) =>
            {
                if (e.GroupId == "env-profile")
                {
                    buttonSettings_Click(null, null);
                }
            };
            ribbon.ItemToggled += (_, e) =>
            {
                if (e.Pressed && (e.ItemId == "rt-xml" || e.ItemId == "rt-json"))
                {
                    AppInstance.Settings.ResponseType = e.ItemId == "rt-json" ? ResponseType.JSON : ResponseType.XML;
                    AppInstance.Save();
                }
            };
            ribbon.FieldChanged += (_, e) => OnRibbonFieldChanged(e);
        }

        /// <summary>Field changes in the Environment tab are saved as "Use and Save" in Settings would.</summary>
        private void OnRibbonFieldChanged(RibbonFieldChangedEventArgs e)
        {
            var settings = AppInstance.Settings;
            if (e.ItemId == "env-port")
            {
                var text = e.Value?.Trim() ?? string.Empty;
                int? port = null;
                if (text.Length > 0)
                {
                    if (!int.TryParse(text, out var parsed) || parsed < 1 || parsed > 65535)
                    {
                        // put the saved port back
                        ribbon.SetFieldValue("env-port", settings.ActiveProfile.Port?.ToString() ?? string.Empty, EnvironmentTabId);
                        return;
                    }
                    port = parsed;
                }
                if (port == settings.ActiveProfile.Port)
                {
                    return;
                }
                settings.ActiveProfile.Port = port;
                AppInstance.Save();
                RefreshProfileChrome();
            }
            else if (e.ItemId == "env-lang")
            {
                settings.Lang = e.Value == "cs" ? ResponseLang.CS : e.Value == "sk" ? ResponseLang.SK : ResponseLang.Default;
                AppInstance.Save();
            }
        }
    }
}
