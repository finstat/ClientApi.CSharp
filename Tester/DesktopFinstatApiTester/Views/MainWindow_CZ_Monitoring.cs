extern alias CZ;

using System.Threading.Tasks;

namespace DesktopFinstatApiTester.Views
{
    public partial class MainWindow
    {
        private Task CZMonitoringICOAddRequest() => DoApiRequestAsync("MonitoringICOAdd", "CZ", CZMonitoringICOAdd, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "IČO"),
            new ApiCallParameter(ParameterTypeEnum.String, "Category", (data) => true)
        });

        private object CZMonitoringICOAdd(object[] parameters)
        {
            var client = CreateCZApiMonitoringClient();
            var result = client.Add((string)parameters[0], (string)parameters[1], IsJSON(), RequestLang()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private Task CZMonitoringICORemoveRequest() => DoApiRequestAsync("MonitoringICORemove", "CZ", CZMonitoringICORemove, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "IČO"),
            new ApiCallParameter(ParameterTypeEnum.String, "Category", (data) => true)
        });

        private object CZMonitoringICORemove(object[] parameters)
        {
            var client = CreateCZApiMonitoringClient();
            var result = client.Remove((string)parameters[0], (string)parameters[1], IsJSON(), RequestLang()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private Task CZMonitoringICOListRequest() => DoApiRequestAsync("MonitoringICOList", "CZ", CZMonitoringICOList, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "Category", (data) => true)
        });

        private object CZMonitoringICOList(object[] parameters)
        {
            var client = CreateCZApiMonitoringClient();
            var result = client.GetMonitorings((string)parameters[0], IsJSON(), RequestLang()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private Task CZMonitoringICOReportRequest() => DoApiRequestAsync("MonitoringICOReport", "CZ", CZMonitoringICOReport, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "Category", (data) => true)
        });

        private object CZMonitoringICOReport(object[] parameters)
        {
            var client = CreateCZApiMonitoringClient();
            var result = client.GetReport((string)parameters[0], IsJSON(), RequestLang()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private Task CZMonitoringCategoriesRequest() =>
            DoApiRequestAsync("MonitoringCategories", "CZ", CZMonitoringCategories);

        private object CZMonitoringCategories(object[] parameters)
        {
            var client = CreateCZApiMonitoringClient();
            var result = client.GetCategories(IsJSON(), RequestLang()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }
    }
}
