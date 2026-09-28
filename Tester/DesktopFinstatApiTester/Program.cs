using System;
using Avalonia;

namespace DesktopFinstatApiTester
{
    internal static class Program
    {
        // Avalonia needs an explicit entry point (WPF generated one from App.xaml's StartupUri).
        [STAThread]
        public static void Main(string[] args) =>
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
