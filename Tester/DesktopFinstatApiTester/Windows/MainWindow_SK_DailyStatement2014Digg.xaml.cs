using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace DesktopFinstatApiTester.Windows
{
    public partial class MainWindow : Fluent.RibbonWindow
    {
        private void buttonDailyStatement2014DiffList_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("DailyStatement2014DiffList", "SK", SKDailyStatement2014DiffList);
        }

        private object SKDailyStatement2014DiffList(object[] parameters)
        {
            var client = CreateSKApiDailyStatement2014DiffClient();
            var result = client.RequestListOfDailyStatement2014Diffs(IsJSON());
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonDailyStatement2014DiffFile_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("DailyStatement2014DiffFile", "SK", SKDailyStatement2014DiffFile, new[] {
                new ApiCallParameter(ParameterTypeEnum.String, "File"),
                new ApiCallParameter(ParameterTypeEnum.Folder, "Select Save Folder")
            });
        }

        private object SKDailyStatement2014DiffFile(object[] parameters)
        {
            var client = CreateSKApiDailyStatement2014DiffClient();
            var result = client.DownloadDailyStatement2014DiffFile((string)parameters[0], (string)parameters[1]);
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonOpenDailyStatement2014DiffFile_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("Open DailyStatement2014DiffFile", "SK", (parameters) =>
            {
                using (var stream = File.OpenRead((string)parameters[0]))
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false))
                {
                    var firstItem = archive.Entries.First();
                    XmlSerializer serializer = new XmlSerializer(typeof(FinstatApi.ViewModel.Diff.Statement.StatementResult[]));
                    return (FinstatApi.ViewModel.Diff.Statement.StatementResult[])serializer.Deserialize(firstItem.Open());
                }
            }, new[] {
                new ApiCallParameter(ParameterTypeEnum.File, "Open Zip File")
            });
        }

        private void buttonOpenDailyStatement2014DiffLegend_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("DailyStatementDiff2014Legend", "SK", SKDailyStatementDiff2014Legend);
        }

        private object SKDailyStatementDiff2014Legend(object[] parameters)
        {
            var client = CreateSKApiDailyStatement2014DiffClient();
            var result = client.RequestStatement2014Legend();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }
    }
}
