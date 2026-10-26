using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DesktopFinstatApiTester.Views
{
    public partial class MainWindow
    {
        private Task SKDailyStatementDiffListRequest() =>
            DoApiRequestAsync("DailyStatementDiffList", "SK", SKDailyStatementDiffList);

        private object SKDailyStatementDiffList(object[] parameters)
        {
            var client = CreateSKApiDailyStatementDiffClient();
            var result = client.RequestListOfDailyStatementDiffs(IsJSON()).GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }

        private Task SKDailyStatementDiffFileRequest() => DoApiRequestAsync("DailyStatementDiffFile", "SK", SKDailyStatementDiffFile, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "File"),
            new ApiCallParameter(ParameterTypeEnum.Folder, "Select Save Folder")
        });

        private object SKDailyStatementDiffFile(object[] parameters)
        {
            var client = CreateSKApiDailyStatementDiffClient();
            var result = client.DownloadDailyStatementDiffFile((string)parameters[0], (string)parameters[1]).GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }

        private Task OpenDailyStatementDiffFileRequest() => DoApiRequestAsync("Open DailyStatementDiffFile", "SK", (parameters) =>
        {
            using (var stream = File.OpenRead((string)parameters[0]))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false))
            {
                var firstItem = archive.Entries.First();
                XmlSerializer serializer = new XmlSerializer(typeof(FinstatApi.ViewModel.Diff.StatementResult[]));
                return (FinstatApi.ViewModel.Diff.StatementResult[])serializer.Deserialize(firstItem.Open());
            }
        }, new[] {
            new ApiCallParameter(ParameterTypeEnum.File, "Open Zip File")
        });

        private Task SKDailyStatementDiffLegendRequest() =>
            DoApiRequestAsync("DailyStatementDiffLegend", "SK", SKDailyStatementDiffLegend);

        private object SKDailyStatementDiffLegend(object[] parameters)
        {
            var client = CreateSKApiDailyStatementDiffClient();
            var result = client.RequestStatementLegend().GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }
    }
}
