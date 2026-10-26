extern alias CZ;

using System.Threading.Tasks;

namespace DesktopFinstatApiTester.Views
{
    public partial class MainWindow
    {
        private Task CZMonitoringDateAddRequest() => DoApiRequestAsync("MonitoringDateAdd", "CZ", CZMonitoringDateAdd, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "Date"),
            new ApiCallParameter(ParameterTypeEnum.String, "Category", (data) => true),
            LangParameter()
        });

        private object CZMonitoringDateAdd(object[] parameters)
        {
            var client = CreateCZApiMonitoringClient();
            var result = client.AddDate((string)parameters[0], (string)parameters[1], IsJSON(), RequestLang(parameters)).GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }

        private Task CZMonitoringDateRemoveRequest() => DoApiRequestAsync("MonitoringDateRemove", "CZ", CZMonitoringDateRemove, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "Date"),
            new ApiCallParameter(ParameterTypeEnum.String, "Category", (data) => true),
            LangParameter()
        });

        private object CZMonitoringDateRemove(object[] parameters)
        {
            var client = CreateCZApiMonitoringClient();
            var result = client.RemoveDate((string)parameters[0], (string)parameters[1], IsJSON(), RequestLang(parameters)).GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }

        private Task CZMonitoringDateListRequest() => DoApiRequestAsync("MonitoringDateList", "CZ", CZMonitoringDateList, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "Category", (data) => true),
            LangParameter()
        });

        private object CZMonitoringDateList(object[] parameters)
        {
            var client = CreateCZApiMonitoringClient();
            var result = client.GetDateMonitorings((string)parameters[0], IsJSON(), RequestLang(parameters)).GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }

        private Task CZMonitoringDateReportRequest() => DoApiRequestAsync("MonitoringDateReport", "CZ", CZMonitoringDateReport, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "Category", (data) => true),
            LangParameter()
        });

        private object CZMonitoringDateReport(object[] parameters)
        {
            var client = CreateCZApiMonitoringClient();
            var result = client.GetDateReport((string)parameters[0], IsJSON(), RequestLang(parameters)).GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }
    }
}
