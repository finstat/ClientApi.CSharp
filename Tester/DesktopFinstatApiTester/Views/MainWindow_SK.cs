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

        private void UpdateLimits(FinstatApi.ViewModel.Limits limits)
        {
            if (IsActiveProfileCall())
            {
                AppInstance.Limits.FromModel(limits);
            }
        }

        /// <summary>Wire the shared per-request instrumentation every SK client needs.</summary>
        private T ConfigureSKClient<T>(T client, Model.EnvProfile profile) where T : FinstatApi.CommonAbstractApiClient
        {
            ConfigureDevSslHandler(client, profile.SkUrl);
            client.OnRequest += Client_OnRequest;
            client.OnResponse += Client_OnResponse;
            client.OnErrorResponseContent += Client_OnResponseContent;
            client.OnResponseContent += Client_OnResponseContent;
            return client;
        }

        private FinstatApi.ApiClient CreateSKApiClient(Model.EnvProfile profile = null) =>
            ConfigureSKClient(new FinstatApi.ApiClient(CallProfile(profile).SkUrl, AppInstance.Settings.PublicKeyFor(CallProfile(profile)), AppInstance.Settings.PrivateKeyFor(CallProfile(profile)), AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut), CallProfile(profile));

        private FinstatApi.ApiMonitoringClient CreateSKApiMonitoringClient(Model.EnvProfile profile = null) =>
            ConfigureSKClient(new FinstatApi.ApiMonitoringClient(CallProfile(profile).SkUrl, AppInstance.Settings.PublicKeyFor(CallProfile(profile)), AppInstance.Settings.PrivateKeyFor(CallProfile(profile)), AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut), CallProfile(profile));

        private FinstatApi.ApiDailyDiffClient CreateSKApiDailyDiffClient(Model.EnvProfile profile = null) =>
            ConfigureSKClient(new FinstatApi.ApiDailyDiffClient(CallProfile(profile).SkUrl, AppInstance.Settings.PublicKeyFor(CallProfile(profile)), AppInstance.Settings.PrivateKeyFor(CallProfile(profile)), AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut), CallProfile(profile));

        private FinstatApi.ApiDailyStatementDiffClient CreateSKApiDailyStatementDiffClient(Model.EnvProfile profile = null) =>
            ConfigureSKClient(new FinstatApi.ApiDailyStatementDiffClient(CallProfile(profile).SkUrl, AppInstance.Settings.PublicKeyFor(CallProfile(profile)), AppInstance.Settings.PrivateKeyFor(CallProfile(profile)), AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut), CallProfile(profile));

        private FinstatApi.ApiDailyStatement2014DiffClient CreateSKApiDailyStatement2014DiffClient(Model.EnvProfile profile = null) =>
            ConfigureSKClient(new FinstatApi.ApiDailyStatement2014DiffClient(CallProfile(profile).SkUrl, AppInstance.Settings.PublicKeyFor(CallProfile(profile)), AppInstance.Settings.PrivateKeyFor(CallProfile(profile)), AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut), CallProfile(profile));

        private FinstatApi.ApiStatementClient CreateSKApiStatementClient(Model.EnvProfile profile = null) =>
            ConfigureSKClient(new FinstatApi.ApiStatementClient(CallProfile(profile).SkUrl, AppInstance.Settings.PublicKeyFor(CallProfile(profile)), AppInstance.Settings.PrivateKeyFor(CallProfile(profile)), AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut), CallProfile(profile));

        private FinstatApi.ApiReportingClient CreateSKApiReportingClient(Model.EnvProfile profile = null) =>
            ConfigureSKClient(new FinstatApi.ApiReportingClient(CallProfile(profile).SkUrl, AppInstance.Settings.PublicKeyFor(CallProfile(profile)), AppInstance.Settings.PrivateKeyFor(CallProfile(profile)), AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut), CallProfile(profile));

        private FinstatApi.ApiDailyUltimateDiffClient CreateSKApiDailyUltimateDiffClient(Model.EnvProfile profile = null) =>
            ConfigureSKClient(new FinstatApi.ApiDailyUltimateDiffClient(CallProfile(profile).SkUrl, AppInstance.Settings.PublicKeyFor(CallProfile(profile)), AppInstance.Settings.PrivateKeyFor(CallProfile(profile)), AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut), CallProfile(profile));

        private FinstatApi.ApiDistraintClient CreateSKApiDistraintClient(Model.EnvProfile profile = null) =>
            ConfigureSKClient(new FinstatApi.ApiDistraintClient(CallProfile(profile).SkUrl, AppInstance.Settings.PublicKeyFor(CallProfile(profile)), AppInstance.Settings.PrivateKeyFor(CallProfile(profile)), AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut), CallProfile(profile));

        private FinstatApi.ApiBankruptcyRestructuringClient CreateSKApiBankruptcyRestructuringClient(Model.EnvProfile profile = null) =>
            ConfigureSKClient(new FinstatApi.ApiBankruptcyRestructuringClient(CallProfile(profile).SkUrl, AppInstance.Settings.PublicKeyFor(CallProfile(profile)), AppInstance.Settings.PrivateKeyFor(CallProfile(profile)), AppInstance.Settings.StationID, AppInstance.Settings.StationName, AppInstance.Settings.TimeOut), CallProfile(profile));
    }
}
