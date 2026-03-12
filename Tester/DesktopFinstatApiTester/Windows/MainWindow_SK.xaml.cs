using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows;

namespace DesktopFinstatApiTester.Windows
{
    public partial class MainWindow : Fluent.RibbonWindow
    {
        private void ConfigureDevSslHandler(FinstatApi.CommonAbstractApiClient client, string url)
        {
#if DEBUG
            if (url.Contains("zdrojak.eu") || url.Contains("zdrojakcz.eu") || url.Contains("localhost"))
            {
                client.HttpClientHandlerFactory = () =>
                {
                    X509Certificate2 cert = null;
                    X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                    store.Open(OpenFlags.ReadOnly);
                    X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindByIssuerName, "root_ca_dev_zdrojak.eu", false);
                    if (certs.Count > 0)
                    {
                        cert = certs[0];
                    }
                    else
                    {
                        X509Store storeMachine = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                        storeMachine.Open(OpenFlags.ReadOnly);
                        X509Certificate2Collection certsMachine = storeMachine.Certificates.Find(X509FindType.FindByIssuerName, "root_ca_dev_zdrojak.eu", false);
                        if (certsMachine.Count > 0)
                        {
                            cert = certsMachine[0];
                        }
                    }
                    var handler = new HttpClientHandler();
                    handler.ServerCertificateCustomValidationCallback = (message, serverCert, chain, errors) => true;
                    if (cert != null)
                    {
                        handler.ClientCertificates.Add(cert);
                    }
                    return handler;
                };
            }
#endif
        }

        private FinstatApi.ApiClient CreateSKApiClient()
        {
            var client = new FinstatApi.ApiClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut);
            ConfigureDevSslHandler(client, AppInstance.Settings.FinStatApiUrl);
            client.OnRequest += Client_OnRequest;
            client.OnResponse += Client_OnResponse;
            client.OnErrorResponseContent += Client_OnResponseContent;
            return client;
        }

        private FinstatApi.ApiMonitoringClient CreateSKApiMonitoringClient()
        {
            var client = new FinstatApi.ApiMonitoringClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut);
            ConfigureDevSslHandler(client, AppInstance.Settings.FinStatApiUrl);
            client.OnRequest += Client_OnRequest;
            client.OnResponse += Client_OnResponse;
            client.OnErrorResponseContent += Client_OnResponseContent;
            return client;
        }

        private FinstatApi.ApiDailyDiffClient CreateSKApiDailyDiffClient()
        {
            var client = new FinstatApi.ApiDailyDiffClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut);
            ConfigureDevSslHandler(client, AppInstance.Settings.FinStatApiUrl);
            client.OnRequest += Client_OnRequest;
            client.OnResponse += Client_OnResponse;
            client.OnErrorResponseContent += Client_OnResponseContent;
            return client;
        }

        private FinstatApi.ApiDailyStatementDiffClient CreateSKApiDailyStatementDiffClient()
        {
            var client = new FinstatApi.ApiDailyStatementDiffClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut);
            ConfigureDevSslHandler(client, AppInstance.Settings.FinStatApiUrl);
            client.OnRequest += Client_OnRequest;
            client.OnResponse += Client_OnResponse;
            client.OnErrorResponseContent += Client_OnResponseContent;
            return client;
        }

        private FinstatApi.ApiDailyStatement2014DiffClient CreateSKApiDailyStatement2014DiffClient()
        {
            var client = new FinstatApi.ApiDailyStatement2014DiffClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut);
            ConfigureDevSslHandler(client, AppInstance.Settings.FinStatApiUrl);
            client.OnRequest += Client_OnRequest;
            client.OnResponse += Client_OnResponse;
            client.OnErrorResponseContent += Client_OnResponseContent;
            return client;
        }

        private FinstatApi.ApiStatementClient CreateSKApiStatementClient()
        {
            var client = new FinstatApi.ApiStatementClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut);
            ConfigureDevSslHandler(client, AppInstance.Settings.FinStatApiUrl);
            client.OnRequest += Client_OnRequest;
            client.OnResponse += Client_OnResponse;
            client.OnErrorResponseContent += Client_OnResponseContent;
            return client;
        }

        private FinstatApi.ApiReportingClient CreateSKApiReportingClient()
        {
            var client = new FinstatApi.ApiReportingClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut);
            ConfigureDevSslHandler(client, AppInstance.Settings.FinStatApiUrl);
            client.OnRequest += Client_OnRequest;
            client.OnResponse += Client_OnResponse;
            client.OnErrorResponseContent += Client_OnResponseContent;
            return client;
        }

        private FinstatApi.ApiDailyUltimateDiffClient CreateSKApiDailyUltimateDiffClient()
        {
            var client = new FinstatApi.ApiDailyUltimateDiffClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut);
            ConfigureDevSslHandler(client, AppInstance.Settings.FinStatApiUrl);
            client.OnRequest += Client_OnRequest;
            client.OnResponse += Client_OnResponse;
            client.OnErrorResponseContent += Client_OnResponseContent;
            return client;
        }

        private FinstatApi.ApiDistraintClient CreateSKApiDistraintClient()
        {
            var client = new FinstatApi.ApiDistraintClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut);
            ConfigureDevSslHandler(client, AppInstance.Settings.FinStatApiUrl);
            client.OnRequest += Client_OnRequest;
            client.OnResponse += Client_OnResponse;
            client.OnErrorResponseContent += Client_OnResponseContent;
            return client;
        }
        private FinstatApi.ApiBankruptcyRestructuringClient CreateSKApiBankruptcyRestructuringClient()
        {
            var client = new FinstatApi.ApiBankruptcyRestructuringClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut);
            ConfigureDevSslHandler(client, AppInstance.Settings.FinStatApiUrl);
            client.OnRequest += Client_OnRequest;
            client.OnResponse += Client_OnResponse;
            client.OnErrorResponseContent += Client_OnResponseContent;
            return client;
        }
    }
}
