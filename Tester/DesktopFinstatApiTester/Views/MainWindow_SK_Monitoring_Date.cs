using System.Threading.Tasks;

namespace DesktopFinstatApiTester.Views
{
    public partial class MainWindow
    {
        private Task SKMonitoringDateAddRequest() => DoApiRequestAsync("MonitoringDateAdd", "SK", SKMonitoringDateAdd, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "Date"),
            new ApiCallParameter(ParameterTypeEnum.String, "Category", (data) => true)
        });

        private object SKMonitoringDateAdd(object[] parameters)
        {
            var client = CreateSKApiMonitoringClient();
            var result = client.AddDate((string)parameters[0], (string)parameters[1], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private Task SKMonitoringDateRemoveRequest() => DoApiRequestAsync("MonitoringDateRemove", "SK", SKMonitoringDateRemove, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "Date"),
            new ApiCallParameter(ParameterTypeEnum.String, "Category", (data) => true)
        });

        private object SKMonitoringDateRemove(object[] parameters)
        {
            var client = CreateSKApiMonitoringClient();
            var result = client.RemoveDate((string)parameters[0], (string)parameters[1], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private Task SKMonitoringDateListRequest() => DoApiRequestAsync("MonitoringDateList", "SK", SKMonitoringDateList, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "Category", (data) => true)
        });

        private object SKMonitoringDateList(object[] parameters)
        {
            var client = CreateSKApiMonitoringClient();
            var result = client.GetDateMonitorings((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private Task SKMonitoringDateReportRequest() => DoApiRequestAsync("MonitoringDateReport", "SK", SKMonitoringDateReport, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "Category", (data) => true)
        });

        private object SKMonitoringDateReport(object[] parameters)
        {
            var client = CreateSKApiMonitoringClient();
            var result = client.GetDateReport((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private Task SKMonitoringDateProceedingsRequest() =>
            DoApiRequestAsync("MonitoringDateProceedings", "SK", SKMonitoringDateProceedings);

        private object SKMonitoringDateProceedings(object[] parameters)
        {
            var client = CreateSKApiMonitoringClient();
            var result = client.GetDateProceedings(IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }
    }
}
