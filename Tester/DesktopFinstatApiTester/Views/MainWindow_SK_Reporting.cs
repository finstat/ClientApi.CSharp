using System.Threading.Tasks;

namespace DesktopFinstatApiTester.Views
{
    public partial class MainWindow
    {
        private Task SKReportingTopicsRequest() => DoApiRequestAsync("ReportingTopics", "SK", SKReportingTopics);

        private object SKReportingTopics(object[] parameters)
        {
            var client = CreateSKApiReportingClient();
            var result = client.RequestTopics().GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private Task SKReportingListRequest() => DoApiRequestAsync("ReportingList", "SK", SKReportingList, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "Topic"),
        });

        private object SKReportingList(object[] parameters)
        {
            var client = CreateSKApiReportingClient();
            var result = client.RequestList((string)parameters[0]).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private Task SKDownloadReportingOutputRequest() => DoApiRequestAsync("DownloadReportingOutput", "SK", SKDownloadReportingOutput, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "File Name"),
            new ApiCallParameter(ParameterTypeEnum.Folder, "Select Save Folder")
        });

        private object SKDownloadReportingOutput(object[] parameters)
        {
            var client = CreateSKApiReportingClient();
            var result = client.DownloadReportFile((string)parameters[0], (string)parameters[1]).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }
    }
}
