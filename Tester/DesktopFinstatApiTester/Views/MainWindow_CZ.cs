extern alias CZ;

using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

namespace DesktopFinstatApiTester.Views
{
    public partial class MainWindow
    {
        private void ConfigureDevSslHandlerCZ(CZ::FinstatApi.CommonAbstractApiClient client, string url)
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

        private T ConfigureCZClient<T>(T client) where T : CZ::FinstatApi.CommonAbstractApiClient
        {
            ConfigureDevSslHandlerCZ(client, AppInstance.Settings.FinStatApiUrlCZ);
            client.OnRequest += Client_OnRequest;
            client.OnResponse += Client_OnResponse;
            client.OnErrorResponseContent += Client_OnResponseContent;
            return client;
        }

        private CZ::FinstatApi.ApiClient CreateCZApiClient() =>
            ConfigureCZClient(new CZ::FinstatApi.ApiClient(AppInstance.Settings.FinStatApiUrlCZ, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut));

        private CZ::FinstatApi.ApiMonitoringClient CreateCZApiMonitoringClient() =>
            ConfigureCZClient(new CZ::FinstatApi.ApiMonitoringClient(AppInstance.Settings.FinStatApiUrlCZ, AppInstance.Settings.ApiKeys.PublicKey, AppInstance.Settings.ApiKeys.PrivateKey, AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut));
    }
}
