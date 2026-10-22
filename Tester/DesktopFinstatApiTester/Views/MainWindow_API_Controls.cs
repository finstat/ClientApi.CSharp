using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Avalonia.Controls;

using Avalonia.Interactivity;

using FinstatApi;

namespace DesktopFinstatApiTester.Views
{
    /// <summary>
    /// The two response surfaces: the history grid (select a row to inspect it, or right-click for the
    /// curl command / a repeat) and the object-graph tree (right-click a node to copy or serialize it).
    /// </summary>
    public partial class MainWindow
    {
        #region Control-DataGridResponse

        private void datagridResponse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = datagridResponse.SelectedItem as ViewModel.ResponseItem;

            // Filling a tab that hasn't been realized yet pulls focus into it, which makes the
            // TabControl switch to it — so remember the active tab and put it back.
            var activeTab = tabControlResponse.SelectedIndex;

            objectTreeData.Source = (item != null && item.DataCount > 0) ? item.Data[0] : null;

            // A dictionary rather than a wrapper object, so the two header bags show under their own
            // names and ObjectTree renders each one's entries directly.
            objectTreeHeaders.Source = (item == null) ? null : new Dictionary<string, object>
            {
                ["RequestHeaders"] = item.RequestHeaders,
                ["ResponseHeaders"] = item.ResponseHeaders,
            };

            tabControlResponse.SelectedIndex = activeTab;
        }

        private void dataGridShowResponseOutputWindow_Click(object sender, RoutedEventArgs e) =>
            _ = FireAsync(async () =>
            {
                var item = datagridResponse.SelectedItem as ViewModel.ResponseItem;
                if (item != null && item.Content != null && item.Content.Any())
                {
                    await ShowOutputAsync("Response body", Encoding.UTF8.GetString(item.Content));
                }
                else
                {
                    await _dialogs.AlertAsync("No Response for this request", "Attention");
                }
            });

        private void dataGridCurlCommand_Click(object sender, RoutedEventArgs e) =>
            _ = FireAsync(ShowCurlCommandAsync);

        private async Task ShowCurlCommandAsync()
        {
            var item = datagridResponse.SelectedItem as ViewModel.ResponseItem;
            if (item == null)
            {
                await _dialogs.AlertAsync("Nothing selected", "Attention");
                return;
            }

            var endPoint = string.Empty;
            var hashParam = string.Empty;
            var parameters = item.Parameters;
            var urlParameters = new Dictionary<string, object>();
            switch (item.Request)
            {
                case "Autocomplete":
                    endPoint = "autocomplete";
                    hashParam = (string)parameters.First();
                    urlParameters.Add("query", parameters.First());
                    break;
                case "AutoLogIn":
                    endPoint = "autologin";
                    hashParam = "autologin";
                    urlParameters.Add("url", parameters.First());
                    break;
                case "DailyDiffList":
                    endPoint = "GetListOfDiffs";
                    hashParam = null;
                    break;
                case "DailyDiffFile":
                    endPoint = "GetFile";
                    hashParam = (string)parameters.First();
                    urlParameters.Add("filename", parameters.First());
                    break;
                case "DailyStatement2014DiffList":
                    endPoint = "GetListOfStatement2014Diffs";
                    hashParam = null;
                    break;
                case "DailyStatement2014DiffFile":
                    endPoint = "GetStatement2014File";
                    hashParam = (string)parameters.First();
                    urlParameters.Add("filename", parameters.First());
                    break;
                case "DailyStatementDiff2014Legend":
                    endPoint = "GetStatement2014Legend";
                    hashParam = "sk";// lang
                    urlParameters.Add("lang", "sk");
                    break;
                case "DailyStatementDiffList":
                    endPoint = "GetListOfStatementDiffs";
                    hashParam = null;
                    break;
                case "DailyStatementDiffFile":
                    endPoint = "GetStatementFile";
                    hashParam = (string)parameters.First();
                    urlParameters.Add("filename", parameters.First());
                    break;
                case "DailyStatementDiffLegend":
                    endPoint = "GetStatementLegend";
                    hashParam = "sk";// lang
                    urlParameters.Add("lang", "sk");
                    break;
                case "DailyUltimateDiffList":
                    endPoint = "GetListOfUltimateDiffs";
                    hashParam = null;
                    break;
                case "DailyUltimateDiffFile":
                    endPoint = "GetUltimateFile";
                    hashParam = (string)parameters.First();
                    urlParameters.Add("filename", parameters.First());
                    break;
                case "Basic":
                    endPoint = "basic";
                    hashParam = (string)parameters.First();
                    urlParameters.Add("ico", parameters.First());
                    break;
                case "Detail":
                    endPoint = "detail";
                    hashParam = (string)parameters.First();
                    urlParameters.Add("ico", parameters.First());
                    break;
                case "PremiumCZ":
                    endPoint = "premiumcz";
                    hashParam = (string)parameters.First();
                    urlParameters.Add("ico", parameters.First());
                    break;
                case "Extended":
                    endPoint = "extended";
                    hashParam = (string)parameters.First();
                    urlParameters.Add("ico", parameters.First());
                    break;
                case "EliteCZ":
                    endPoint = "elitecz";
                    hashParam = (string)parameters.First();
                    urlParameters.Add("ico", parameters.First());
                    break;
                case "UltimateCZ":
                    endPoint = "ultimatecz";
                    hashParam = (string)parameters.First();
                    urlParameters.Add("ico", parameters.First());
                    break;
                case "Ultimate":
                    endPoint = "ultimate";
                    hashParam = (string)parameters.First();
                    urlParameters.Add("ico", parameters.First());
                    break;
                case "DistraintSearch":
                    endPoint = "distraintSearch";
                    hashParam = string.Join("|", parameters);
                    urlParameters.Add("search", string.Join("|", parameters));
                    break;
                case "DistraintDetail":
                    endPoint = "distraintDetail";
                    hashParam = string.Join(string.Empty, parameters);
                    urlParameters.Add("search", string.Join(",", parameters));
                    break;
                case "DistraintResults":
                    endPoint = "distraintResults";
                    hashParam = string.Join("|", parameters);
                    urlParameters.Add("search", string.Join("|", parameters));
                    break;
                case "DistraintResultsByToken":
                    endPoint = "distraintResultsByToken";
                    hashParam = (string)parameters.First();
                    urlParameters.Add("token", parameters.First());
                    break;
                case "DistraintStoredDetail":
                    endPoint = "distraintStoredDetail";
                    hashParam = (string)parameters.First();
                    urlParameters.Add("id", parameters.First());
                    break;
                case "MonitoringCategories":
                    endPoint = "MonitoringCategories";
                    hashParam = "monitoringcategories";
                    break;
                case "MonitoringICOAdd":
                    endPoint = "AddToMonitoring";
                    hashParam = (string)parameters.First();
                    urlParameters.Add("ico", parameters.First());
                    break;
                case "MonitoringICORemove":
                    endPoint = "RemoveFromMonitoring";
                    hashParam = (string)parameters.First();
                    urlParameters.Add("ico", parameters.First());
                    break;
                case "MonitoringICOList":
                    endPoint = "MonitoringList";
                    hashParam = "list";
                    break;
                case "MonitoringICOReport":
                    endPoint = "MonitoringReport";
                    hashParam = "report";
                    break;
                case "MonitoringICOProceedings":
                    endPoint = "MonitoringProceedings";
                    hashParam = "proceedings";
                    break;
                case "MonitoringDateAdd":
                    endPoint = "AddDateToMonitoring";
                    hashParam = (string)parameters.First();
                    urlParameters.Add("date", parameters.First());
                    if (parameters.ElementAtOrDefault(1) is string addDateCat && !string.IsNullOrEmpty(addDateCat))
                        urlParameters.Add("category", addDateCat);
                    break;
                case "MonitoringDateRemove":
                    endPoint = "RemoveDateFromMonitoring";
                    hashParam = (string)parameters.First();
                    urlParameters.Add("date", parameters.First());
                    if (parameters.ElementAtOrDefault(1) is string rmDateCat && !string.IsNullOrEmpty(rmDateCat))
                        urlParameters.Add("category", rmDateCat);
                    break;
                case "MonitoringDateList":
                    endPoint = "MonitoringDateList";
                    hashParam = "datelist";
                    if (parameters.ElementAtOrDefault(0) is string listDateCat && !string.IsNullOrEmpty(listDateCat))
                        urlParameters.Add("category", listDateCat);
                    break;
                case "MonitoringDateReport":
                    endPoint = "MonitoringDateReport";
                    hashParam = "datereport";
                    if (parameters.ElementAtOrDefault(0) is string rptDateCat && !string.IsNullOrEmpty(rptDateCat))
                        urlParameters.Add("category", rptDateCat);
                    break;
                case "MonitoringDateProceedings":
                    endPoint = "MonitoringDateProceedings";
                    hashParam = "dateproceedings";
                    break;
                case "ReportingTopics":
                    endPoint = "GetReportingTopics";
                    hashParam = "reporting-topics";
                    break;
                case "ReportingList":
                    endPoint = "GetReportingList";
                    hashParam = $"reporting-list|{parameters.First()}";
                    urlParameters.Add("topic", parameters.First());
                    break;
                case "DownloadReportingOutput":
                    endPoint = "GetReportingOutput";
                    hashParam = (string)parameters.First();
                    urlParameters.Add("FileName", parameters.First());
                    break;
                case "StatementList":
                    endPoint = "GetStatements";
                    hashParam = (string)parameters.First();
                    urlParameters.Add("ico", parameters.First());
                    break;
                case "StatementDetail":
                    endPoint = "GetStatementDetail";
                    hashParam = string.Join("|", parameters?.Take(2));
                    urlParameters.Add("ico", parameters.First());
                    urlParameters.Add("year", parameters.Skip(1).First());
                    urlParameters.Add("template", parameters.Skip(2).First());
                    break;
                case "StatementTemplateLegend":
                    endPoint = "GetStatementTemplateLegend";
                    hashParam = (string)parameters.Last(); // lang
                    urlParameters.Add("lang", parameters.First());
                    urlParameters.Add("template", parameters.Skip(1).First());
                    break;
                case "PersonBankruptcyProceedings":
                    endPoint = "PersonBankruptcyProceedings";
                    hashParam = $"{parameters.First()}|{parameters.Skip(1).First()}|{(parameters.Skip(2).First() as DateTime?)?.ToString("yyyy-MM-dd")}";
                    urlParameters.Add("name", parameters.First());
                    urlParameters.Add("surname", parameters.Skip(1).First());
                    urlParameters.Add("dateofbirth", (parameters.Skip(2).First() as DateTime?)?.ToString("yyyy-MM-dd"));
                    break;
                case "CompanyBankruptcyRestructuring":
                    endPoint = "CompanyBankruptcyRestructuring";
                    hashParam = string.Join("", parameters?.Take(2));
                    urlParameters.Add("ico", parameters.First());
                    urlParameters.Add("name", parameters.Skip(1).First());
                    break;
            }

            if (string.IsNullOrEmpty(endPoint))
            {
                await _dialogs.AlertAsync($"No suitable API method for '{item.Request}'", "Attention");
                return;
            }

            var userApiKey = AppInstance.Settings.ApiKeys.PublicKey;
            var privateKey = AppInstance.Settings.ApiKeys.PrivateKey;
            var calculatedHash = CommonAbstractApiClient.ComputeVerificationHash(userApiKey, privateKey, hashParam);

            var url = (item.ApiSource == "CZ")
                ? AppInstance.Settings.FinStatApiUrlCZ
                : AppInstance.Settings.FinStatApiUrl;
            url = url.TrimEnd('/');
            var urlSuffix = IsJSON() ? ".json" : null;

            urlParameters.Add("apiKey", userApiKey);
            urlParameters.Add("hash", calculatedHash);
            urlParameters.Add("StationId", "curl-test");
            urlParameters.Add("StationName", "curl-test");
            var lang = RequestLang(parameters);
            if (item.ApiSource == "CZ" && lang != null && !urlParameters.ContainsKey("lang"))
            {
                urlParameters.Add("Lang", lang);
            }

            var str = new StringBuilder();
            str.AppendLine($"curl '{url}/{endPoint}{urlSuffix}?{string.Join("&", urlParameters.Select(x => $"{x.Key}={x.Value}"))}' -v");
            str.AppendLine();
            str.AppendLine($"curl -X POST '{url}/{endPoint}{urlSuffix}' -d '{string.Join("&", urlParameters.Select(x => $"{x.Key}={x.Value}"))}' -v ");
            str.AppendLine();
            var dataString = string.Join(",", urlParameters.Select(x => $"\"{x.Key}\":\"{x.Value}\""));
            str.AppendLine($"curl -H \"Content-Type: application/json\" '{url}/{endPoint}{urlSuffix}' -d '{{{dataString}}}' -v");

            await ShowOutputAsync("Curl", str.ToString());
        }

        private void dataGridRepeatRequest_Click(object sender, RoutedEventArgs e) =>
            _ = FireAsync(RepeatRequestAsync);

        private async Task RepeatRequestAsync()
        {
            var item = datagridResponse.SelectedItem as ViewModel.ResponseItem;
            if (item == null)
            {
                await _dialogs.AlertAsync("Nothing selected", "Attention");
                return;
            }

            Func<object[], object> apiCall = null;
            switch (item.Request)
            {
                case "Autocomplete":
                    switch (item.ApiSource)
                    {
                        case "CZ": apiCall = CZAutoComplete; break;
                        case "SK": apiCall = SKAutoComplete; break;
                    }
                    break;
                case "AutoLogIn":
                    switch (item.ApiSource)
                    {
                        case "CZ": apiCall = CZAutoLogin; break;
                        case "SK": apiCall = SKAutoLogin; break;
                    }
                    break;
                case "DailyDiffList":
                    if (item.ApiSource == "SK") apiCall = SKDailyDiffList;
                    break;
                case "DailyDiffFile":
                    if (item.ApiSource == "SK") apiCall = SKDailyDiffFile;
                    break;
                case "DailyStatement2014DiffList":
                    if (item.ApiSource == "SK") apiCall = SKDailyStatement2014DiffList;
                    break;
                case "DailyStatement2014DiffFile":
                    if (item.ApiSource == "SK") apiCall = SKDailyStatement2014DiffFile;
                    break;
                case "DailyStatementDiff2014Legend":
                    if (item.ApiSource == "SK") apiCall = SKDailyStatementDiff2014Legend;
                    break;
                case "DailyStatementDiffList":
                    if (item.ApiSource == "SK") apiCall = SKDailyStatementDiffList;
                    break;
                case "DailyStatementDiffFile":
                    if (item.ApiSource == "SK") apiCall = SKDailyStatementDiffFile;
                    break;
                case "DailyStatementDiffLegend":
                    if (item.ApiSource == "SK") apiCall = SKDailyStatementDiffLegend;
                    break;
                case "DailyUltimateDiffList":
                    if (item.ApiSource == "SK") apiCall = SKDailyUltimateDiffList;
                    break;
                case "DailyUltimateDiffFile":
                    if (item.ApiSource == "SK") apiCall = SKDailyUltimateDiffFile;
                    break;
                case "Basic":
                    switch (item.ApiSource)
                    {
                        case "CZ": apiCall = CZBasic; break;
                        case "SK": apiCall = SKBasic; break;
                    }
                    break;
                case "Detail":
                    switch (item.ApiSource)
                    {
                        case "CZ": apiCall = CZDetail; break;
                        case "SK": apiCall = SKDetail; break;
                    }
                    break;
                case "PremiumCZ":
                    if (item.ApiSource == "CZ") apiCall = CZPremium;
                    break;
                case "Extended":
                    if (item.ApiSource == "SK") apiCall = SKExtended;
                    break;
                case "EliteCZ":
                    if (item.ApiSource == "CZ") apiCall = CZElite;
                    break;
                case "UltimateCZ":
                    if (item.ApiSource == "CZ") apiCall = CZUltimate;
                    break;
                case "Ultimate":
                    if (item.ApiSource == "SK") apiCall = SKUltimate;
                    break;
                case "DistraintSearch":
                    if (item.ApiSource == "SK") apiCall = SKDistraintSearch;
                    break;
                case "DistraintDetail":
                    if (item.ApiSource == "SK") apiCall = SKDistraintDetail;
                    break;
                case "DistraintResults":
                    if (item.ApiSource == "SK") apiCall = SKDistraintResults;
                    break;
                case "DistraintResultsByToken":
                    if (item.ApiSource == "SK") apiCall = SKDistraintResultsByToken;
                    break;
                case "DistraintStoredDetail":
                    if (item.ApiSource == "SK") apiCall = SKDistraintStoredDetail;
                    break;
                case "MonitoringCategories":
                    switch (item.ApiSource)
                    {
                        case "CZ": apiCall = CZMonitoringCategories; break;
                        case "SK": apiCall = SKMonitoringCategories; break;
                    }
                    break;
                case "MonitoringICOAdd":
                    switch (item.ApiSource)
                    {
                        case "CZ": apiCall = CZMonitoringICOAdd; break;
                        case "SK": apiCall = SKMonitoringICOAdd; break;
                    }
                    break;
                case "MonitoringICORemove":
                    switch (item.ApiSource)
                    {
                        case "CZ": apiCall = CZMonitoringICORemove; break;
                        case "SK": apiCall = SKMonitoringICORemove; break;
                    }
                    break;
                case "MonitoringICOList":
                    switch (item.ApiSource)
                    {
                        case "CZ": apiCall = CZMonitoringICOList; break;
                        case "SK": apiCall = SKMonitoringICOList; break;
                    }
                    break;
                case "MonitoringICOReport":
                    switch (item.ApiSource)
                    {
                        case "CZ": apiCall = CZMonitoringICOReport; break;
                        case "SK": apiCall = SKMonitoringICOReport; break;
                    }
                    break;
                case "MonitoringICOProceedings":
                    // CZ has no Proceedings variant.
                    if (item.ApiSource == "SK") apiCall = SKMonitoringICOProceedings;
                    break;
                case "MonitoringDateAdd":
                    switch (item.ApiSource)
                    {
                        case "CZ": apiCall = CZMonitoringDateAdd; break;
                        case "SK": apiCall = SKMonitoringDateAdd; break;
                    }
                    break;
                case "MonitoringDateRemove":
                    switch (item.ApiSource)
                    {
                        case "CZ": apiCall = CZMonitoringDateRemove; break;
                        case "SK": apiCall = SKMonitoringDateRemove; break;
                    }
                    break;
                case "MonitoringDateList":
                    switch (item.ApiSource)
                    {
                        case "CZ": apiCall = CZMonitoringDateList; break;
                        case "SK": apiCall = SKMonitoringDateList; break;
                    }
                    break;
                case "MonitoringDateReport":
                    switch (item.ApiSource)
                    {
                        case "CZ": apiCall = CZMonitoringDateReport; break;
                        case "SK": apiCall = SKMonitoringDateReport; break;
                    }
                    break;
                case "MonitoringDateProceedings":
                    // CZ has no Proceedings variant.
                    if (item.ApiSource == "SK") apiCall = SKMonitoringDateProceedings;
                    break;
                case "ReportingTopics":
                    if (item.ApiSource == "SK") apiCall = SKReportingTopics;
                    break;
                case "ReportingList":
                    if (item.ApiSource == "SK") apiCall = SKReportingList;
                    break;
                case "DownloadReportingOutput":
                    if (item.ApiSource == "SK") apiCall = SKDownloadReportingOutput;
                    break;
                case "StatementList":
                    if (item.ApiSource == "SK") apiCall = SKStatementList;
                    break;
                case "StatementDetail":
                    if (item.ApiSource == "SK") apiCall = SKStatementDetail;
                    break;
                case "StatementTemplateLegend":
                    if (item.ApiSource == "SK") apiCall = SKStatementTemplateLegend;
                    break;
                case "PersonBankruptcyProceedings":
                    if (item.ApiSource == "SK") apiCall = SKPersonBankruptcyProceedings;
                    break;
                case "CompanyBankruptcyRestructuring":
                    if (item.ApiSource == "SK") apiCall = SKCompanyBankruptcyRestructuring;
                    break;
            }

            if (apiCall == null)
            {
                await _dialogs.AlertAsync("No suitable API method", "Attention");
                return;
            }

            await RunApiRequestAsync(item.Request, item.ApiSource, apiCall, item.Parameters);
        }

        #endregion

        #region Control-ObjectTree

        /// <summary>
        /// The node the user has selected in the Data tree, or <c>null</c> when nothing is selected.
        /// <c>SelectedPath</c> is what distinguishes "no selection" from "a selected node whose value
        /// happens to be null" — both leave <c>SelectedValue</c> null.
        /// </summary>
        private bool TryGetSelectedNode(out object value, out string path)
        {
            value = objectTreeData.SelectedValue;
            path = objectTreeData.SelectedPath;
            return path != null;
        }

        /// <summary>Report "nothing selected" once, rather than in every handler.</summary>
        private async Task<bool> RequireSelectionAsync()
        {
            if (objectTreeData.SelectedPath != null)
            {
                return true;
            }
            await _dialogs.AlertAsync("Select a node in the Data tree first", "Attention");
            return false;
        }

        // nulls left out for the same reason the Data tree hides them
        private static readonly JsonSerializerOptions s_nodeJsonOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };

        private static string ToJson(object value) => JsonSerializer.Serialize(value, s_nodeJsonOptions);

        private void treeCopyValueToClipboard_Click(object sender, RoutedEventArgs e) =>
            _ = FireAsync(async () =>
            {
                if (!await RequireSelectionAsync()) return;
                TryGetSelectedNode(out var value, out _);
                await SetClipboardAsync(value?.ToString());
            });

        private void treeCopyToClipboard_Click(object sender, RoutedEventArgs e) =>
            _ = FireAsync(async () =>
            {
                if (!await RequireSelectionAsync()) return;
                TryGetSelectedNode(out var value, out _);
                await SetClipboardAsync(ToJson(value));
            });

        private void treeShowJSONInOutputWindow_Click(object sender, RoutedEventArgs e) =>
            _ = FireAsync(async () =>
            {
                if (!await RequireSelectionAsync()) return;
                TryGetSelectedNode(out var value, out var path);
                await ShowOutputAsync($"JSON — {path}", ToJson(value));
            });

        private void treeShowXMLInOutputWindow_Click(object sender, RoutedEventArgs e) =>
            _ = FireAsync(async () =>
            {
                if (!await RequireSelectionAsync()) return;
                TryGetSelectedNode(out var value, out var path);
                if (value == null)
                {
                    await _dialogs.AlertAsync($"'{path}' is null — nothing to serialize", "Attention");
                    return;
                }

                // XmlSerializer rejects plenty of shapes a response graph contains (interfaces,
                // dictionaries, types without a parameterless ctor), so a failure here is expected
                // rather than exceptional — say so instead of throwing up a stack trace.
                string xml;
                try
                {
                    using var ms = new MemoryStream();
                    new XmlSerializer(value.GetType()).Serialize(ms, value);
                    ms.Position = 0;
                    using var reader = new StreamReader(ms);
                    xml = reader.ReadToEnd();
                }
                catch (Exception ex)
                {
                    await _dialogs.AlertAsync(
                        $"'{path}' ({value.GetType().Name}) can't be serialized to XML: {ex.Message}",
                        "Attention");
                    return;
                }
                await ShowOutputAsync($"XML — {path}", xml);
            });

        #endregion
    }
}
