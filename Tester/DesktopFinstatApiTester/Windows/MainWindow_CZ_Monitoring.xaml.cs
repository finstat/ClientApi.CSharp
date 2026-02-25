extern alias CZ;
using System.Windows;

namespace DesktopFinstatApiTester.Windows
{
    public partial class MainWindow : Fluent.RibbonWindow
    {
        private void buttonCZMonitoringIcoAdd_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("MonitoringICOAdd", "CZ", CZMonitoringICOAdd, new[] {
                new ApiCallParameter(ParameterTypeEnum.String, "IČO"),
                new ApiCallParameter(ParameterTypeEnum.String, "Category", (data) => true)
            });
        }

        private object CZMonitoringICOAdd(object[] parameters)
        {
            var client = CreateCZApiMonitoringClient();
            var result = client.Add((string)parameters[0], (string)parameters[1], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonCZMonitoringIcoRemove_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("MonitoringICORemove", "CZ", CZMonitoringICORemove, new[] {
                new ApiCallParameter(ParameterTypeEnum.String, "IČO"),
                new ApiCallParameter(ParameterTypeEnum.String, "Category", (data) => true)
            });
        }

        private object CZMonitoringICORemove(object[] parameters)
        {
            var client = CreateCZApiMonitoringClient();
            var result = client.Remove((string)parameters[0], (string)parameters[1], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonCZMonitoringIcoList_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("MonitoringICOList", "CZ", CZMonitoringICOList, new[] {
                new ApiCallParameter(ParameterTypeEnum.String, "Category", (data) => true)
            });
        }

        private object CZMonitoringICOList(object[] parameters)
        {
            var client = CreateCZApiMonitoringClient();
            var result = client.GetMonitorings((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonCZMonitoringIcoReport_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("MonitoringICOReport", "CZ", CZMonitoringICOReport, new[] {
                new ApiCallParameter(ParameterTypeEnum.String, "Category", (data) => true)
            });
        }

        private object CZMonitoringICOReport(object[] parameters)
        {
            var client = CreateCZApiMonitoringClient();
            var result = client.GetReport((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }


        private void buttonCZMonitoringCategories_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("MonitoringCategories", "CZ", CZMonitoringCategories);
        }

        private object CZMonitoringCategories(object[] parameters)
        {
            var client = CreateCZApiMonitoringClient();
            var result = client.GetCategories(IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }
    }
}
