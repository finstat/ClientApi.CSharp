using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DesktopFinstatApiTester.Views
{
    public partial class MainWindow
    {
        private Task SKDailyDiffListRequest() => DoApiRequestAsync("DailyDiffList", "SK", SKDailyDiffList);

        private object SKDailyDiffList(object[] parameters)
        {
            var client = CreateSKApiDailyDiffClient();
            var result = client.RequestListOfDailyDiffs(IsJSON()).GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }

        private Task SKDailyDiffFileRequest() => DoApiRequestAsync("DailyDiffFile", "SK", SKDailyDiffFile, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "File"),
            new ApiCallParameter(ParameterTypeEnum.Folder, "Select Save Folder")
        });

        private object SKDailyDiffFile(object[] parameters)
        {
            var client = CreateSKApiDailyDiffClient();
            var result = client.DownloadDailyDiffFile((string)parameters[0], (string)parameters[1]).GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }

        private Task OpenDailyDiffFileRequest() => DoApiRequestAsync("Open DailyDiffFile", "SK", (parameters) =>
        {
            using (var stream = File.OpenRead((string)parameters[0]))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false))
            {
                var firstItem = archive.Entries.First();
                XmlSerializer serializer = new XmlSerializer(typeof(FinstatApi.ViewModel.Diff.ExtendedResult[]));
                return (FinstatApi.ViewModel.Diff.ExtendedResult[])serializer.Deserialize(firstItem.Open());
            }
        }, new[] {
            new ApiCallParameter(ParameterTypeEnum.File, "Open Zip File")
        });
    }
}
