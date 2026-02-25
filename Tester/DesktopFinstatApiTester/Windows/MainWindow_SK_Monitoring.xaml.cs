using System;
using System.Windows;

namespace DesktopFinstatApiTester.Windows
{
    public partial class MainWindow : Fluent.RibbonWindow
    {
        private void buttonMonitoringIcoAdd_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("MonitoringICOAdd", "SK", SKMonitoringICOAdd, new[] {
                new ApiCallParameter(ParameterTypeEnum.String, "IČO"),
                new ApiCallParameter(ParameterTypeEnum.String, "Category", (data) => true)
            });
        }

        private object SKMonitoringICOAdd(object[] parameters)
        {
            var client = CreateSKApiMonitoringClient();
            var result = client.Add((string)parameters[0], (string)parameters[1], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonMonitoringIcoRemove_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("MonitoringICORemove", "SK", SKMonitoringICORemove, new[] {
                new ApiCallParameter(ParameterTypeEnum.String, "IČO"),
                new ApiCallParameter(ParameterTypeEnum.String, "Category", (data) => true)
            });
        }

        private object SKMonitoringICORemove(object[] parameters)
        {
            var client = CreateSKApiMonitoringClient();
            var result = client.Remove((string)parameters[0], (string)parameters[1], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonMonitoringIcoList_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("MonitoringICOList", "SK", SKMonitoringICOList, new[] {
                new ApiCallParameter(ParameterTypeEnum.String, "Category", (data) => true)
            });
        }

        private object SKMonitoringICOList(object[] parameters)
        {
            var client = CreateSKApiMonitoringClient();
            var result = client.GetMonitorings((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonMonitoringIcoReport_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("MonitoringICOReport", "SK", SKMonitoringICOReport, new[] {
                new ApiCallParameter(ParameterTypeEnum.String, "Category", (data) => true)
            });
        }

        private object SKMonitoringICOReport(object[] parameters)
        {
            var client = CreateSKApiMonitoringClient();
            var result = client.GetReport((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonMonitoringIcoProceedings_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("MonitoringICOProceedings", "SK", SKMonitoringICOProceedings);
        }

        private object SKMonitoringICOProceedings(object[] parameters)
        {
            var client = CreateSKApiMonitoringClient();
            var result = client.GetProceedings(IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonMonitoringCategories_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("MonitoringCategories", "SK", SKMonitoringCategories);
        }

        private object SKMonitoringCategories(object[] parameters)
        {
            var client = CreateSKApiMonitoringClient();
            var result = client.GetCategories(IsJSON());
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }
    }
}
