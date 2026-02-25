using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Serialization;

namespace DesktopFinstatApiTester.Windows
{
    public partial class MainWindow : Fluent.RibbonWindow
    {
        private void buttonDailyUltimateDiffList_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("DailyUltimateDiffList", "SK", SKDailyUltimateDiffList);
        }

        private object SKDailyUltimateDiffList(object[] parameters)
        {
            var client = CreateSKApiDailyUltimateDiffClient();
            var result = client.RequestListOfDailyUltimateDiffs(IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonDailyUltimateDiffFile_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("DailyUltimateDiffFile", "SK", SKDailyUltimateDiffFile, new[] {
                new ApiCallParameter(ParameterTypeEnum.String, "File"),
                new ApiCallParameter(ParameterTypeEnum.Folder, "Select Save Folder")
            });
        }

        private object SKDailyUltimateDiffFile(object[] parameters)
        {
            var client = CreateSKApiDailyUltimateDiffClient();
            var result = client.DownloadDailyUltimateDiffFile((string)parameters[0], (string)parameters[1]).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonOpenDailyUltimateDiffFile_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("Open DailyUltimateDiffFile", "SK", (parameters) =>
            {
                using (var stream = File.OpenRead((string)parameters[0]))
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false))
                {
                    var firstItem = archive.Entries.First();
                    XmlSerializer serializer = new XmlSerializer(typeof(FinstatApi.ViewModel.Diff.UltimateResult[]));
                    return (FinstatApi.ViewModel.Diff.UltimateResult[])serializer.Deserialize(firstItem.Open());
                }
            }, new[] {
                new ApiCallParameter(ParameterTypeEnum.File, "Open Zip File")
            });
        }
    }
}
