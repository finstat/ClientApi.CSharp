using DesktopFinstatApiTester.ViewModel;
using FinstatApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;

namespace DesktopFinstatApiTester.Windows
{
    public partial class MainWindow : Fluent.RibbonWindow
    {
        #region Control-DataGridResponse
        private void datagridResponse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            treeViewObjectGraph.ItemsSource = null;
            treeViewHeadersGraph.ItemsSource = null;
            var grid = (DataGrid)sender;
            if (grid.SelectedItems != null && grid.SelectedItems.Count > 0)
            {
                var item = (ViewModel.ResponseItem)grid.SelectedItem;
                if (item != null)
                {
                    var graph = new ViewModel.ObjectViewModelHierarchy((item.DataCount > 0) ? item.Data[0] : null);
                    treeViewObjectGraph.ItemsSource = graph.FirstGeneration;
                    graph.FirstGeneration[0].IsSelected = true;
                    graph.FirstGeneration[0].IsExpanded = true;

                    var graph2 = new ViewModel.ObjectViewModelHierarchy(new BasicResponse
                    {
                        RequestHeaders = item.RequestHeaders,
                        ResponseHeaders = item.ResponseHeaders,
                    });
                    treeViewHeadersGraph.ItemsSource = graph2.FirstGeneration;
                    graph2.FirstGeneration[0].IsSelected = true;
                    graph2.FirstGeneration[0].IsExpanded = true;
                }
            }
        }

        private void dataGridShowResponseOutputWindow_Click(object sender, RoutedEventArgs e)
        {
            if (datagridResponse.SelectedItems != null && datagridResponse.SelectedItems.Count > 0)
            {
                var item = (ViewModel.ResponseItem)datagridResponse.SelectedItem;
                if (item != null && item.Content != null && item.Content.Any())
                {
                    OutputWindow window = new OutputWindow(Encoding.UTF8.GetString(item.Content))
                    {
                        Owner = this,
                        Title = "Response body"
                    };
                    var result = window.ShowDialog();
                }
                else
                {
                    MessageBox.Show("No Response for this request", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                }
            }
        }

        private void dataGridCurlCommand_Click(object sender, RoutedEventArgs e)
        {
            if ((datagridResponse.SelectedItems?.Count ?? 0) == 0)
            {
                MessageBox.Show("Nothing selected", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                return;
            }
            var item = (ViewModel.ResponseItem)datagridResponse.SelectedItem;
            var endPoint = string.Empty;
            var hashParam = string.Empty;
            var parameters = item?.Parameters;
            Dictionary<string, object> urlParameters = new Dictionary<string, object>();
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
                    break;
                case "MonitoringDateRemove":
                    endPoint = "RemoveDateFromMonitoring";
                    hashParam = (string)parameters.First();
                    urlParameters.Add("date", parameters.First());
                    break;
                case "MonitoringDateList":
                    endPoint = "MonitoringDateList";
                    hashParam = "datelist";
                    break;
                case "MonitoringDateReport":
                    endPoint = "MonitoringDateReport";
                    hashParam = "datereport";
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
                MessageBox.Show($"No suitable API method for '{item.Request}'", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
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

            StringBuilder str = new StringBuilder();
            str.AppendLine($"curl '{url}/{endPoint}{urlSuffix}?{string.Join("&", urlParameters.Select(x => $"{x.Key}={x.Value}"))}' -v");
            str.AppendLine();
            str.AppendLine($"curl -X POST '{url}/{endPoint}{urlSuffix}' -d '{string.Join("&", urlParameters.Select(x => $"{x.Key}={x.Value}"))}' -v ");
            str.AppendLine();
            var dataString = string.Join(",", urlParameters.Select(x => $"\"{x.Key}\":\"{x.Value}\""));
            str.AppendLine($"curl -H \"Content-Type: application/json\" '{url}/{endPoint}{urlSuffix}' -d '{{{dataString}}}' -v");
            OutputWindow window = new OutputWindow(str.ToString())
            {
                Owner = this,
                Title = "Curl"
            };
            var result = window.ShowDialog();
        }

        private void dataGridRepeatRequest_Click(object sender, RoutedEventArgs e)
        {
            if ((datagridResponse.SelectedItems?.Count ?? 0) == 0)
            {
                MessageBox.Show("Nothing selected", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                return;
            }
            var item = (ViewModel.ResponseItem)datagridResponse.SelectedItem;
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
                    switch (item.ApiSource)
                    {
                        case "SK": apiCall = SKDailyDiffList; break;
                    }
                    break;
                case "DailyDiffFile":
                    switch (item.ApiSource)
                    {
                        case "SK": apiCall = SKDailyDiffFile; break;
                    }
                    break;
                case "DailyStatement2014DiffList":
                    switch (item.ApiSource)
                    {
                        case "SK": apiCall = SKDailyStatement2014DiffList; break;
                    }
                    break;
                case "DailyStatement2014DiffFile":
                    switch (item.ApiSource)
                    {
                        case "SK": apiCall = SKDailyStatement2014DiffFile; break;
                    }
                    break;
                case "DailyStatementDiff2014Legend":
                    switch (item.ApiSource)
                    {
                        case "SK": apiCall = SKDailyStatementDiff2014Legend; break;
                    }
                    break;
                case "DailyStatementDiffList":
                    switch (item.ApiSource)
                    {
                        case "SK": apiCall = SKDailyStatementDiffList; break;
                    }
                    break;
                case "DailyStatementDiffFile":
                    switch (item.ApiSource)
                    {
                        case "SK": apiCall = SKDailyStatementDiffFile; break;
                    }
                    break;
                case "DailyStatementDiffLegend":
                    switch (item.ApiSource)
                    {
                        case "SK": apiCall = SKDailyStatementDiffLegend; break;
                    }
                    break;
                case "DailyUltimateDiffList":
                    switch (item.ApiSource)
                    {
                        case "SK": apiCall = SKDailyUltimateDiffList; break;
                    }
                    break;
                case "DailyUltimateDiffFile":
                    switch (item.ApiSource)
                    {
                        case "SK": apiCall = SKDailyUltimateDiffFile; break;
                    }
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
                    switch (item.ApiSource)
                    {
                        case "CZ": apiCall = CZPremium; break;
                    }
                    break;
                case "Extended":
                    switch (item.ApiSource)
                    {
                        case "SK": apiCall = SKExtended; break;
                    }
                    break;
                case "Ultimate":
                    switch (item.ApiSource)
                    {
                        case "SK": apiCall = SKUltimate; break;
                    }
                    break;
                case "DistraintSearch":
                    switch (item.ApiSource)
                    {
                        case "SK": apiCall = SKDistraintSearch; break;
                    }
                    break;
                case "DistraintDetail":
                    switch (item.ApiSource)
                    {
                        case "SK": apiCall = SKDistraintDetail; break;
                    }
                    break;
                case "DistraintResults":
                    switch (item.ApiSource)
                    {
                        case "SK": apiCall = SKDistraintResults; break;
                    }
                    break;
                case "DistraintResultsByToken":
                    switch (item.ApiSource)
                    {
                        case "SK": apiCall = SKDistraintResultsByToken; break;
                    }
                    break;
                case "DistraintStoredDetail":
                    switch (item.ApiSource)
                    {
                        case "SK": apiCall = SKDistraintStoredDetail; break;
                    }
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
                    switch (item.ApiSource)
                    {
                        //case "CZ": apiCall = CZMonitoringICOProceedings; break;
                        case "SK": apiCall = SKMonitoringICOProceedings; break;
                    }
                    break;
                case "MonitoringDateAdd":
                    switch (item.ApiSource)
                    {
                        //case "CZ": apiCall = CZMonitoringDateAdd; break;
                        case "SK": apiCall = SKMonitoringDateAdd; break;
                    }
                    break;
                case "MonitoringDateRemove":
                    switch (item.ApiSource)
                    {
                        //case "CZ": apiCall = CZMonitoringDateRemove; break;
                        case "SK": apiCall = SKMonitoringDateRemove; break;
                    }
                    break;
                case "MonitoringDateList":
                    switch (item.ApiSource)
                    {
                        //case "CZ": apiCall = CZMonitoringDateList; break;
                        case "SK": apiCall = SKMonitoringDateList; break;
                    }
                    break;
                case "MonitoringDateReport":
                    switch (item.ApiSource)
                    {
                        //case "CZ": apiCall = CZMonitoringDateReport; break;
                        case "SK": apiCall = SKMonitoringDateReport; break;
                    }
                    break;
                case "MonitoringDateProceedings":
                    switch (item.ApiSource)
                    {
                        //case "CZ": apiCall = CZMonitoringDateProceedings; break;
                        case "SK": apiCall = SKMonitoringDateProceedings; break;
                    }
                    break;
                case "ReportingTopics":
                    switch (item.ApiSource)
                    {
                        case "SK": apiCall = SKReportingTopics; break;
                    }
                    break;
                case "ReportingList":
                    switch (item.ApiSource)
                    {
                        case "SK": apiCall = SKReportingList; break;
                    }
                    break;
                case "DownloadReportingOutput":
                    switch (item.ApiSource)
                    {
                        case "SK": apiCall = SKDownloadReportingOutput; break;
                    }
                    break;
                case "StatementList":
                    switch (item.ApiSource)
                    {
                        case "SK": apiCall = SKStatementList; break;
                    }
                    break;
                case "StatementDetail":
                    switch (item.ApiSource)
                    {
                        case "SK": apiCall = SKStatementDetail; break;
                    }
                    break;
                case "StatementTemplateLegend":
                    switch (item.ApiSource)
                    {
                        case "SK": apiCall = SKStatementTemplateLegend; break;
                    }
                    break;
                case "PersonBankruptcyProceedings":
                    switch (item.ApiSource)
                    {
                        case "SK": apiCall = SKPersonBankruptcyProceedings; break;
                    }
                    break;
                case "CompanyBankruptcyRestructuring":
                    switch (item.ApiSource)
                    {
                        case "SK": apiCall = SKCompanyBankruptcyRestructuring; break;
                    }
                    break;
            }
            if (apiCall == null)
            {
                MessageBox.Show("No suitable API method", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                return;
            }

            RunApiRequest(item.Request, item.ApiSource, apiCall, item.Parameters);

        }

        #endregion

        #region Control-TreeViewObjectGraph
        private void treeViewObjectGraph_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            treeViewObjectGraph.ContextMenu = treeViewObjectGraph.Resources["TreeViewItemContextMenu"] as System.Windows.Controls.ContextMenu;
        }

        private void treeViewCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (treeViewObjectGraph.SelectedItem != null)
            {
                Clipboard.SetText(treeViewObjectGraph.SelectedItem.ToString());
            }
        }

        private void treeViewCopyValueToClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (treeViewObjectGraph.SelectedItem != null)
            {
                var item = (ViewModel.ObjectViewModel)treeViewObjectGraph.SelectedItem;
                Clipboard.SetText(item.Value.ToString());
            }
        }

        private void treeViewShowInOutputWindow_Click(object sender, RoutedEventArgs e)
        {
            if (treeViewObjectGraph.SelectedItem != null)
            {
                OutputWindow window = new OutputWindow(treeViewObjectGraph.SelectedItem.ToString())
                {
                    Owner = this,
                    Title = "Selected Result Node"
                };
                var result = window.ShowDialog();
            }
        }

        private void treeViewShowXMLInOutputWindow_Click(object sender, RoutedEventArgs e)
        {
            if (treeViewObjectGraph.SelectedItem != null)
            {
                var item = (ViewModel.ObjectViewModel)treeViewObjectGraph.SelectedItem;
                MemoryStream ms = new MemoryStream();
                XmlSerializer serializer = new XmlSerializer(item.Object.GetType());
                serializer.Serialize(ms, item.Object);
                ms.Position = 0;
                StreamReader r = new StreamReader(ms);
                OutputWindow window = new OutputWindow(r.ReadToEnd())
                {
                    Owner = this,
                    Title = "Selected Result Node"
                };
                var result = window.ShowDialog();
            }
        }

        private void treeViewShowJSONInOutputWindow_Click(object sender, RoutedEventArgs e)
        {
            if (treeViewObjectGraph.SelectedItem != null)
            {
                var item = (ViewModel.ObjectViewModel)treeViewObjectGraph.SelectedItem;
                OutputWindow window = new OutputWindow(JsonSerializer.Serialize(item.Object, new JsonSerializerOptions { WriteIndented = true }))
                {
                    Owner = this,
                    Title = "Selected Result Node"
                };
                var result = window.ShowDialog();
            }
        }

        private void treeViewShowTreeInOutputWindow_Click(object sender, RoutedEventArgs e)
        {
            if (treeViewObjectGraph.SelectedItem != null)
            {
                var item = (ViewModel.ObjectViewModel)treeViewObjectGraph.SelectedItem;
                OutputWindow window = new OutputWindow(item.ToString())
                {
                    Owner = this,
                    Title = "Selected Result Node"
                };
                var result = window.ShowDialog();
            }
        }

        private void treeViewSelect_Click(object sender, RoutedEventArgs e)
        {
            if (treeViewObjectGraph.SelectedItem != null)
            {
                var item = (ViewModel.ObjectViewModel)treeViewObjectGraph.SelectedItem;
                item.IsSelected = !item.IsSelected;
            }
        }

        private void treeViewToggle_Click(object sender, RoutedEventArgs e)
        {
            if (treeViewObjectGraph.SelectedItem != null)
            {
                var item = (ViewModel.ObjectViewModel)treeViewObjectGraph.SelectedItem;
                item.IsExpanded = !item.IsExpanded;
            }
        }
        #endregion
    }
}
