using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Birko.Xaml.Core.Forms;
using DesktopFinstatApiTester.Model;

namespace DesktopFinstatApiTester.Views
{
    /// <summary>
    /// Environment profiles: the switcher in the ribbon's tab row, the status bar in the active profile's
    /// colour, and the profile editor in Settings. Kept in step with the web tester's profile-switcher and
    /// settings modal.
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>Set while the header switcher is being filled, so filling it does not count as a switch.</summary>
        private bool _fillingProfiles;
        private ViewModel.EnvProfileEditor _profileEditor;

        /// <summary>The theme brush of a profile colour; null for none.</summary>
        private static string ColorBrushKey(ProfileColor color)
        {
            switch (color)
            {
                case ProfileColor.Green: return "BColorSuccessBrush";
                case ProfileColor.Amber: return "BColorWarningBrush";
                case ProfileColor.Red: return "BColorDangerBrush";
                default: return null;
            }
        }

        /// <summary>Binds a property to a theme brush, so the colour follows a theme switch; null clears it.</summary>
        private static void BindBrush(Control control, AvaloniaProperty property, string key)
        {
            if (key == null)
            {
                control.ClearValue(property);
                return;
            }
            control.Bind(property, control.GetResourceObservable(key));
        }

        /// <summary>The header switcher, its colour dot and the status bar, after the active profile or the profiles changed.</summary>
        private void RefreshProfileChrome()
        {
            var settings = AppInstance.Settings;
            var active = settings.ActiveProfile;

            _fillingProfiles = true;
            comboBoxProfile.ItemsSource = settings.Profiles.Select(p => p.Name).ToList();
            comboBoxProfile.SelectedIndex = settings.Profiles.IndexOf(active);
            _fillingProfiles = false;
            ToolTip.SetTip(comboBoxProfile, string.Format("SK: {0}\nCZ: {1}", active.SkUrl, active.CzUrl));

            var colorKey = ColorBrushKey(active.Color ?? ProfileColor.None);
            ellipseProfileColor.IsVisible = colorKey != null;
            BindBrush(ellipseProfileColor, Shape.FillProperty, colorKey);

            textBlockStatusBarLeft.Text = string.Format("{0} · SK: {1} · CZ: {2}", active.Name, HostOf(active.SkUrl), HostOf(active.CzUrl));
            // a 3px top edge in the profile's colour, the plain 1px border otherwise
            borderStatusBar.BorderThickness = colorKey != null ? new Thickness(0, 3, 0, 0) : new Thickness(0, 1, 0, 0);
            BindBrush(borderStatusBar, Border.BorderBrushProperty, colorKey ?? "BBorderBrush");

            // the endpoint tooltips and the Environment tab's port show the active profile
            BuildRibbon();
        }

        private static string HostOf(string url) =>
            Uri.TryCreate(url, UriKind.Absolute, out var uri) ? (uri.IsDefaultPort ? uri.Host : uri.Host + ":" + uri.Port) : url;

        private void comboBoxProfile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var settings = AppInstance?.Settings;
            var index = comboBoxProfile.SelectedIndex;
            if (_fillingProfiles || settings == null || index < 0 || index >= settings.Profiles.Count)
            {
                return;
            }
            var id = settings.Profiles[index].Id;
            if (id == settings.ActiveProfileId)
            {
                return;
            }
            settings.ActiveProfileId = id;
            AppInstance.Save();
            RefreshProfileChrome();
        }

        #region Settings — profile editor

        private static IReadOnlyList<FormField> ProfileFields(bool builtIn) => new[]
        {
            new FormField { Name = nameof(ViewModel.EnvProfileEditor.Name),   Label = "Profile name", Required = true, ReadOnly = builtIn },
            new FormField { Name = nameof(ViewModel.EnvProfileEditor.SkHost), Label = "SK API host",  Required = true, ReadOnly = builtIn },
            new FormField { Name = nameof(ViewModel.EnvProfileEditor.CzHost), Label = "CZ API host",  Required = true, ReadOnly = builtIn },
            new FormField { Name = nameof(ViewModel.EnvProfileEditor.Port),   Label = "Port", Type = FieldType.Number, Min = 1, Max = 65535,
                            Placeholder = "443", Hint = "Same port for both hosts; empty = 443." },
            new FormField { Name = nameof(ViewModel.EnvProfileEditor.Color),  Label = "Colour", Type = FieldType.Select,
                            Options = Enum.GetValues(typeof(ProfileColor)).Cast<object>().ToList(),
                            Hint = "Shown next to the profile in the tab row and along the status bar while the profile is active." },
            new FormField { Name = nameof(ViewModel.EnvProfileEditor.PublicKey),  Label = "Public Key for this profile",  Hint = "Empty = the keys above." },
            new FormField { Name = nameof(ViewModel.EnvProfileEditor.PrivateKey), Label = "Private Key for this profile", Type = FieldType.Password, Hint = "Empty = the keys above." },
        };

        /// <summary>Shows one of the working copies in the editor; it becomes the active profile once the settings are used.</summary>
        private void ShowProfileInSettings(string id)
        {
            var profiles = _settingsViewModel.Profiles;
            var profile = EnvProfiles.Find(profiles, id);
            _settingsViewModel.ActiveProfileId = profile.Id;

            _fillingProfiles = true;
            comboBoxProfileEdit.ItemsSource = profiles.Select(p => p.Name).ToList();
            comboBoxProfileEdit.SelectedIndex = profiles.IndexOf(profile);
            _fillingProfiles = false;

            _profileEditor = new ViewModel.EnvProfileEditor(profile);
            profileForm.Model = null;
            profileForm.Fields = ProfileFields(profile.BuiltIn);
            profileForm.Model = _profileEditor;
            buttonProfileDelete.IsEnabled = !profile.BuiltIn;
        }

        private void comboBoxProfileEdit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = comboBoxProfileEdit.SelectedIndex;
            if (_fillingProfiles || _settingsViewModel == null || index < 0 || index >= _settingsViewModel.Profiles.Count)
            {
                return;
            }
            ShowProfileInSettings(_settingsViewModel.Profiles[index].Id);
        }

        private void buttonProfileDuplicate_Click(object sender, RoutedEventArgs e)
        {
            var source = EnvProfiles.Find(_settingsViewModel.Profiles, _settingsViewModel.ActiveProfileId);
            var copy = source.Clone();
            copy.Id = Guid.NewGuid().ToString();
            copy.BuiltIn = false;
            copy.Name = string.Format("{0} (copy)", source.Name);
            _settingsViewModel.Profiles.Add(copy);
            ShowProfileInSettings(copy.Id);
        }

        private void buttonProfileDelete_Click(object sender, RoutedEventArgs e)
        {
            var shown = EnvProfiles.Find(_settingsViewModel.Profiles, _settingsViewModel.ActiveProfileId);
            if (shown.BuiltIn)
            {
                return;
            }
            _settingsViewModel.Profiles.Remove(shown);
            ShowProfileInSettings(_settingsViewModel.Profiles[0].Id);
        }

        #endregion
    }
}
