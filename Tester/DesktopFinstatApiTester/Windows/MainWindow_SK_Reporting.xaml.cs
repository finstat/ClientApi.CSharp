using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DesktopFinstatApiTester.Windows
{
    public partial class MainWindow : Fluent.RibbonWindow
    {
        private void buttonReportingTopics_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("ReportingTopics", "SK", SKReportingTopics);
        }

        private object SKReportingTopics(object[] parameters)
        {
            var client = CreateSKApiReportingClient();
            var result = client.RequestTopics().GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonReportingList_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("ReportingList", "SK", SKReportingList, new[] {
                 new ApiCallParameter(ParameterTypeEnum.String, "Topic"),
            });
        }

        private object SKReportingList(object[] parameters)
        {
            var client = CreateSKApiReportingClient();
            var result = client.RequestList((string)parameters[0]).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonDownloadReportingFile_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("DownloadReportingOutput", "SK", SKDownloadReportingOutput, new[] {
                new ApiCallParameter(ParameterTypeEnum.String, "File Name"),
                new ApiCallParameter(ParameterTypeEnum.Folder, "Select Save Folder")
            });
        }

        private object SKDownloadReportingOutput(object[] parameters)
        {
            var client = CreateSKApiReportingClient();
            var result = client.DownloadReportFile((string)parameters[0], (string)parameters[1]).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }
    }
}
