using System.Threading.Tasks;

namespace DesktopFinstatApiTester.Views
{
    public partial class MainWindow
    {
        private Task SKMonitoringICOAddRequest() => DoApiRequestAsync("MonitoringICOAdd", "SK", SKMonitoringICOAdd, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "IČO"),
            new ApiCallParameter(ParameterTypeEnum.String, "Category", (data) => true)
        });

        private object SKMonitoringICOAdd(object[] parameters)
        {
            var client = CreateSKApiMonitoringClient();
            var result = client.Add((string)parameters[0], (string)parameters[1], IsJSON()).GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }

        private Task SKMonitoringICORemoveRequest() => DoApiRequestAsync("MonitoringICORemove", "SK", SKMonitoringICORemove, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "IČO"),
            new ApiCallParameter(ParameterTypeEnum.String, "Category", (data) => true)
        });

        private object SKMonitoringICORemove(object[] parameters)
        {
            var client = CreateSKApiMonitoringClient();
            var result = client.Remove((string)parameters[0], (string)parameters[1], IsJSON()).GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }

        private Task SKMonitoringICOListRequest() => DoApiRequestAsync("MonitoringICOList", "SK", SKMonitoringICOList, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "Category", (data) => true)
        });

        private object SKMonitoringICOList(object[] parameters)
        {
            var client = CreateSKApiMonitoringClient();
            var result = client.GetMonitorings((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }

        private Task SKMonitoringICOReportRequest() => DoApiRequestAsync("MonitoringICOReport", "SK", SKMonitoringICOReport, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "Category", (data) => true)
        });

        private object SKMonitoringICOReport(object[] parameters)
        {
            var client = CreateSKApiMonitoringClient();
            var result = client.GetReport((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }

        private Task SKMonitoringICOProceedingsRequest() =>
            DoApiRequestAsync("MonitoringICOProceedings", "SK", SKMonitoringICOProceedings);

        private object SKMonitoringICOProceedings(object[] parameters)
        {
            var client = CreateSKApiMonitoringClient();
            var result = client.GetProceedings(IsJSON()).GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }

        private Task SKMonitoringCategoriesRequest() =>
            DoApiRequestAsync("MonitoringCategories", "SK", SKMonitoringCategories);

        private object SKMonitoringCategories(object[] parameters)
        {
            var client = CreateSKApiMonitoringClient();
            var result = client.GetCategories(IsJSON()).GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }
    }
}
