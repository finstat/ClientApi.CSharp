using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DesktopFinstatApiTester.Windows
{
    public partial class MainWindow : Fluent.RibbonWindow
    {
        private void buttonMonitoringDateAdd_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("MonitoringDateAdd", "SK", SKMonitoringDateAdd);
        }

        private object SKMonitoringDateAdd(object[] parameters)
        {
            var client = CreateSKApiMonitoringClient();
            var result = client.AddDate((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonMonitoringDateRemove_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("MonitoringDateRemove", "SK", SKMonitoringDateRemove, new[] {
                new ApiCallParameter(ParameterTypeEnum.String, "Date")
            });
        }

        private object SKMonitoringDateRemove(object[] parameters)
        {
            var client = CreateSKApiMonitoringClient();
            var result = client.RemoveDate((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonMonitoringDateList_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("MonitoringDateList", "SK", SKMonitoringDateList);
        }

        private object SKMonitoringDateList(object[] parameters)
        {
            var client = CreateSKApiMonitoringClient();
            var result = client.GetDateMonitorings(IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonMonitoringDateReport_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("MonitoringDateReport", "SK", SKMonitoringDateReport);
        }

        private object SKMonitoringDateReport(object[] parameters)
        {
            var client = CreateSKApiMonitoringClient();
            var result = client.GetDateReport(IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonMonitoringDateProceedings_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("MonitoringDateProceedings", "SK", SKMonitoringDateProceedings);
        }

        private object SKMonitoringDateProceedings(object[] parameters)
        {
            var client = CreateSKApiMonitoringClient();
            var result = client.GetDateProceedings(IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }
    }
}
