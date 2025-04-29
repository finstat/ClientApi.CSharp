using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DesktopFinstatApiTester.Windows
{
    public partial class MainWindow : Fluent.RibbonWindow
    {
        private FinstatApi.ApiClient CreateSKApiClient()
        {
            var client = new FinstatApi.ApiClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut);
            client.OnRequest += Client_OnRequest;
            client.OnResponse += Client_OnResponse;
            client.OnErrorResponseContent += Client_OnResponseContent;
            return client;
        }

        private FinstatApi.ApiMonitoringClient CreateSKApiMonitoringClient()
        {
            var client = new FinstatApi.ApiMonitoringClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut);
            client.OnRequest += Client_OnRequest;
            client.OnResponse += Client_OnResponse;
            client.OnErrorResponseContent += Client_OnResponseContent;
            return client;
        }

        private FinstatApi.ApiDailyDiffClient CreateSKApiDailyDiffClient()
        {
            var client = new FinstatApi.ApiDailyDiffClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut);
            client.OnRequest += Client_OnRequest;
            client.OnResponse += Client_OnResponse;
            client.OnErrorResponseContent += Client_OnResponseContent;
            return client;
        }

        private FinstatApi.ApiDailyStatementDiffClient CreateSKApiDailyStatementDiffClient()
        {
            var client = new FinstatApi.ApiDailyStatementDiffClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut);
            client.OnRequest += Client_OnRequest;
            client.OnResponse += Client_OnResponse;
            client.OnErrorResponseContent += Client_OnResponseContent;
            return client;
        }

        private FinstatApi.ApiDailyStatement2014DiffClient CreateSKApiDailyStatement2014DiffClient()
        {
            var client = new FinstatApi.ApiDailyStatement2014DiffClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut);
            client.OnRequest += Client_OnRequest;
            client.OnResponse += Client_OnResponse;
            client.OnErrorResponseContent += Client_OnResponseContent;
            return client;
        }

        private FinstatApi.ApiStatementClient CreateSKApiStatementClient()
        {
            var client = new FinstatApi.ApiStatementClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut);
            client.OnRequest += Client_OnRequest;
            client.OnResponse += Client_OnResponse;
            client.OnErrorResponseContent += Client_OnResponseContent;
            return client;
        }

        private FinstatApi.ApiReportingClient CreateSKApiReportingClient()
        {
            var client = new FinstatApi.ApiReportingClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut);
            client.OnRequest += Client_OnRequest;
            client.OnResponse += Client_OnResponse;
            client.OnErrorResponseContent += Client_OnResponseContent;
            return client;
        }

        private FinstatApi.ApiDailyUltimateDiffClient CreateSKApiDailyUltimateDiffClient()
        {
            var client = new FinstatApi.ApiDailyUltimateDiffClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut);
            client.OnRequest += Client_OnRequest;
            client.OnResponse += Client_OnResponse;
            client.OnErrorResponseContent += Client_OnResponseContent;
            return client;
        }

        private FinstatApi.ApiDistraintClient CreateSKApiDistraintClient()
        {
            var client = new FinstatApi.ApiDistraintClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut);
            client.OnRequest += Client_OnRequest;
            client.OnResponse += Client_OnResponse;
            client.OnErrorResponseContent += Client_OnResponseContent;
            return client;
        }
        private FinstatApi.ApiBankruptcyRestructuringClient CreateSKApiBankruptcyRestructuringClient()
        {
            var client = new FinstatApi.ApiBankruptcyRestructuringClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut);
            client.OnRequest += Client_OnRequest;
            client.OnResponse += Client_OnResponse;
            client.OnErrorResponseContent += Client_OnResponseContent;
            return client;
        }
    }
}
