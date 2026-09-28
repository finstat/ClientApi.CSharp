using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

namespace DesktopFinstatApiTester.Views
{
    public partial class MainWindow
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

        /// <summary>Wire the shared per-request instrumentation every SK client needs.</summary>
        private T ConfigureSKClient<T>(T client) where T : FinstatApi.CommonAbstractApiClient
        {
            ConfigureDevSslHandler(client, AppInstance.Settings.FinStatApiUrl);
            client.OnRequest += Client_OnRequest;
            client.OnResponse += Client_OnResponse;
            client.OnErrorResponseContent += Client_OnResponseContent;
            return client;
        }

        private FinstatApi.ApiClient CreateSKApiClient() =>
            ConfigureSKClient(new FinstatApi.ApiClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut));

        private FinstatApi.ApiMonitoringClient CreateSKApiMonitoringClient() =>
            ConfigureSKClient(new FinstatApi.ApiMonitoringClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut));

        private FinstatApi.ApiDailyDiffClient CreateSKApiDailyDiffClient() =>
            ConfigureSKClient(new FinstatApi.ApiDailyDiffClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut));

        private FinstatApi.ApiDailyStatementDiffClient CreateSKApiDailyStatementDiffClient() =>
            ConfigureSKClient(new FinstatApi.ApiDailyStatementDiffClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut));

        private FinstatApi.ApiDailyStatement2014DiffClient CreateSKApiDailyStatement2014DiffClient() =>
            ConfigureSKClient(new FinstatApi.ApiDailyStatement2014DiffClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut));

        private FinstatApi.ApiStatementClient CreateSKApiStatementClient() =>
            ConfigureSKClient(new FinstatApi.ApiStatementClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut));

        private FinstatApi.ApiReportingClient CreateSKApiReportingClient() =>
            ConfigureSKClient(new FinstatApi.ApiReportingClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut));

        private FinstatApi.ApiDailyUltimateDiffClient CreateSKApiDailyUltimateDiffClient() =>
            ConfigureSKClient(new FinstatApi.ApiDailyUltimateDiffClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut));

        private FinstatApi.ApiDistraintClient CreateSKApiDistraintClient() =>
            ConfigureSKClient(new FinstatApi.ApiDistraintClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut));

        private FinstatApi.ApiBankruptcyRestructuringClient CreateSKApiBankruptcyRestructuringClient() =>
            ConfigureSKClient(new FinstatApi.ApiBankruptcyRestructuringClient(AppInstance.Settings.FinStatApiUrl, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut));
    }
}
