extern alias CZ;

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace DesktopFinstatApiTester.Views
{
    /// <summary>
    /// The About modal. Versions are read from the assemblies actually loaded (the tester, both FinStat
    /// client libraries, Avalonia), the same source the web tester's <c>GET api/about</c> uses, so no number
    /// here is maintained by hand.
    /// </summary>
    public partial class MainWindow
    {
        private bool _aboutVersionsFilled;

        private void buttonAbout_Click(object sender, RoutedEventArgs e)
        {
            FillAboutVersions();
            aboutModal.IsOpen = true;
        }

        private void buttonAboutClose_Click(object sender, RoutedEventArgs e) =>
            aboutModal.IsOpen = false;

        /// <summary>Opens the link a button carries in its <c>Tag</c> in the system browser.</summary>
        private async void aboutLink_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Control)?.Tag is not string url || !Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return;
            }
            try
            {
                await Launcher.LaunchUriAsync(uri);
            }
            catch (Exception ex)
            {
                await ShowExceptionAsync(ex);
            }
        }

        private void FillAboutVersions()
        {
            if (_aboutVersionsFilled)
            {
                return;
            }
            _aboutVersionsFilled = true;

            var rows = new (string Label, string Value)[]
            {
                ("Tester", VersionOf(typeof(MainWindow).Assembly)),
                ("FinStat client SK", VersionOf(typeof(FinstatApi.ApiClient).Assembly)),
                ("FinStat client CZ", VersionOf(typeof(CZ::FinstatApi.ApiClient).Assembly)),
                ("Avalonia", VersionOf(typeof(Window).Assembly)),
                ("Runtime", RuntimeInformation.FrameworkDescription),
            };
            for (int i = 0; i < rows.Length; i++)
            {
                gridAboutVersions.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                var label = new TextBlock { Text = rows[i].Label, Margin = new Avalonia.Thickness(0, 1, 16, 1) };
                label.Bind(TextBlock.ForegroundProperty, label.GetResourceObservable("BTextSecondaryBrush"));
                var value = new TextBlock { Text = rows[i].Value, Margin = new Avalonia.Thickness(0, 1) };
                value.Bind(TextBlock.FontFamilyProperty, value.GetResourceObservable("BFontMono"));
                Grid.SetRow(label, i);
                Grid.SetRow(value, i);
                Grid.SetColumn(value, 1);
                gridAboutVersions.Children.Add(label);
                gridAboutVersions.Children.Add(value);
            }
        }

        /// <summary>Informational version without the "+commit" build metadata the SDK appends.</summary>
        private static string VersionOf(Assembly assembly)
        {
            var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                          ?? assembly.GetName().Version?.ToString()
                          ?? "?";
            var plus = version.IndexOf('+');
            return plus >= 0 ? version.Substring(0, plus) : version;
        }
    }
}
