using System.Globalization;
using System.Net;
using System.Threading;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Birko.Xaml.Avalonia.Theming;
using DesktopFinstatApiTester.Views;

namespace DesktopFinstatApiTester
{
    public partial class App : Application
    {
        private ViewModel.ApiApplication _instance = null;

        /// <summary>
        /// The single application view-model (settings + limits + response history). Created lazily
        /// on first access, exactly as the WPF app did, so culture and the certificate policy are in
        /// place before any API client exists.
        /// </summary>
        public ViewModel.ApiApplication Instance
        {
            get
            {
                if (_instance == null)
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("sk-SK");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("sk-SK");

                    // Dev/test API hosts use a private CA; SocketsHttpHandler honours this as the
                    // fallback when a handler supplies no callback of its own.
                    ServicePointManager.ServerCertificateValidationCallback =
                        delegate { return true; };

                    _instance = new ViewModel.ApiApplication();
                }
                return _instance;
            }
        }

        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            // Finstat branding for now. Set before the first window so nothing paints in Light and
            // then re-resolves; widening the offer means merging more sheets in App.axaml and
            // passing them to the MainWindow's theme manager.
            RequestedThemeVariant = BirkoThemeVariants.Finstat;

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}
