extern alias CZ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Birko.Xaml.Avalonia.Dialogs;
using Birko.Xaml.Avalonia.Theming;
using Birko.Xaml.Core.Dialogs;
using Birko.Xaml.Core.Forms;
using Birko.Xaml.Core.Theming;
using DesktopFinstatApiTester.Model;

namespace DesktopFinstatApiTester.Views
{
    /// <summary>
    /// The tester workspace: a Birko ribbon of API commands over the request history grid and the
    /// response object-graph inspector. Chrome, tokens and dialogs come from Birko.Xaml; the API
    /// calls live in the MainWindow_{SK,CZ}_* partials.
    /// </summary>
    public partial class MainWindow : Window
    {
        protected enum ParameterTypeEnum
        {
            String,
            Int,
            Folder,
            File,
            Prompt,
            Pick,
            None,
            Date,
        }

        protected class ApiCallParameter
        {
            public ParameterTypeEnum Type { get; set; } = ParameterTypeEnum.String;
            public string Title { get; set; }
            public IEnumerable<object> Values { get; set; }
            public Func<object, bool> ValidFunction { get; set; } = null;

            public ApiCallParameter(Func<object, bool> validFunction = null) : this(string.Empty, validFunction) { }
            public ApiCallParameter(string title = null, Func<object, bool> validFunction = null) : this(ParameterTypeEnum.String, title, validFunction) { }
            public ApiCallParameter(ParameterTypeEnum type = ParameterTypeEnum.String, string title = null, Func<object, bool> validFunction = null)
            {
                Type = type;
                Title = title;
                ValidFunction = validFunction;
            }
        }

        private readonly IDialogService _dialogs;
        private readonly IThemeManager _themes;
        private ViewModel.Settings _settingsViewModel;

        protected ViewModel.ApiApplication AppInstance =>
            (Avalonia.Application.Current as App)?.Instance;

        public MainWindow()
        {
            InitializeComponent();

            // Dialogs render as token-styled overlays inside the window's root panel, so the API
            // code can await them the same way it awaited a modal MessageBox.
            _dialogs = new DialogService(() => rootPanel);
            // Only Finstat is on offer for now. Stated explicitly rather than detected, because the
            // merged core sheet also carries light + dark (which Finstat needs as its base) and
            // detection would surface those as switchable.
            _themes = new AvaloniaThemeManager(available: new[] { BirkoThemes.FinstatTheme });

            DataContext = AppInstance;

            BuildRibbon();
            InitializeRibbonExtras();
            InitializeComparisons();
            InitializeThemeSwitcher();

            // Ctrl+K opens the palette of every ribbon command.
            AddHandler(KeyDownEvent, OnWindowKeyDown, RoutingStrategies.Tunnel);

            LoadSettingsViewModel();
            RefreshProfileChrome();
            if (string.IsNullOrEmpty(AppInstance?.Settings?.PublicKeyFor(AppInstance.Settings.ActiveProfile)))
            {
                settingsModal.IsOpen = true;
            }
        }

        #region Helpers

        private async Task SetClipboardAsync(string text)
        {
            var clipboard = GetTopLevel(this)?.Clipboard;
            if (clipboard != null)
            {
                await clipboard.SetTextAsync(text ?? string.Empty);
            }
        }

        /// <summary>Show <paramref name="text"/> in a resizable output window (curl commands, raw
        /// bodies, serialized nodes, stack traces).</summary>
        private Task ShowOutputAsync(string title, string text)
        {
            var window = new OutputWindow(text) { Title = title };
            return window.ShowDialog(this);
        }

        public async Task ShowExceptionAsync(Exception ex, string message = null)
        {
            if (ex == null)
            {
                await _dialogs.AlertAsync("An error occured", "Error");
                return;
            }

            var extitle = "Error";
            var failType = (string)null;
            var variant = DialogVariant.Danger;
            if (ex is FinstatApi.FinstatApiException skEx)
            {
                extitle = "FinStat SK API Exception";
                failType = skEx.FailType.ToString();
                variant = VariantForFailType(skEx.FailType);
            }
            else if (ex is CZ::FinstatApi.FinstatApiException czEx)
            {
                extitle = "FinStat CZ API Exception";
                failType = czEx.FailType.ToString();
                variant = VariantForCzFailType(czEx.FailType);
            }
            else if (ex is WebException)
            {
                extitle = "Web Exception";
            }
            else if (ex is TimeoutException)
            {
                extitle = "Timeout Exception";
            }

            if (!string.IsNullOrEmpty(failType))
            {
                extitle = string.Format("{0} — {1}", extitle, failType);
            }

            var body = (!string.IsNullOrEmpty(message)) ? message : ex.Message;
            var wantsDetail = await _dialogs.ConfirmAsync(
                body + Environment.NewLine + Environment.NewLine + "Want to see more details?",
                new ConfirmOptions
                {
                    Title = extitle,
                    ConfirmText = "Show details",
                    CancelText = "Close",
                    Variant = variant,
                });

            if (wantsDetail)
            {
                var sb = new StringBuilder();
                if (!string.IsNullOrEmpty(failType))
                {
                    sb.Append("FailType: ").AppendLine(failType);
                    sb.AppendLine("-------------");
                }
                sb.AppendLine(ex.Message).AppendLine("-------------").Append(ex.StackTrace);
                await ShowOutputAsync(extitle, sb.ToString());
            }
        }

        /// <summary>A recoverable/expected failure gets the softer primary styling; anything else is
        /// a hard error. Mirrors the icon choice the WPF dialog made.</summary>
        private static DialogVariant VariantForFailType(FinstatApi.FinstatApiException.FailTypeEnum ft)
        {
            switch (ft)
            {
                case FinstatApi.FinstatApiException.FailTypeEnum.NotFound:
                case FinstatApi.FinstatApiException.FailTypeEnum.TooShort:
                case FinstatApi.FinstatApiException.FailTypeEnum.BadRequest:
                case FinstatApi.FinstatApiException.FailTypeEnum.GdprRestriction:
                case FinstatApi.FinstatApiException.FailTypeEnum.LimitExceed:
                case FinstatApi.FinstatApiException.FailTypeEnum.Unauthorized:
                case FinstatApi.FinstatApiException.FailTypeEnum.NotValidCustomerKey:
                case FinstatApi.FinstatApiException.FailTypeEnum.InvalidHash:
                case FinstatApi.FinstatApiException.FailTypeEnum.InsufficientAccess:
                case FinstatApi.FinstatApiException.FailTypeEnum.LicenseExpired:
                case FinstatApi.FinstatApiException.FailTypeEnum.AccessDisabled:
                    return DialogVariant.Primary;
                default:
                    return DialogVariant.Danger;
            }
        }

        private static DialogVariant VariantForCzFailType(CZ::FinstatApi.FinstatApiException.FailTypeEnum ft)
        {
            switch (ft)
            {
                case CZ::FinstatApi.FinstatApiException.FailTypeEnum.NotFound:
                case CZ::FinstatApi.FinstatApiException.FailTypeEnum.TooShort:
                case CZ::FinstatApi.FinstatApiException.FailTypeEnum.BadRequest:
                case CZ::FinstatApi.FinstatApiException.FailTypeEnum.GdprRestriction:
                case CZ::FinstatApi.FinstatApiException.FailTypeEnum.LimitExceed:
                case CZ::FinstatApi.FinstatApiException.FailTypeEnum.Unauthorized:
                case CZ::FinstatApi.FinstatApiException.FailTypeEnum.NotValidCustomerKey:
                case CZ::FinstatApi.FinstatApiException.FailTypeEnum.InvalidHash:
                case CZ::FinstatApi.FinstatApiException.FailTypeEnum.InsufficientAccess:
                case CZ::FinstatApi.FinstatApiException.FailTypeEnum.LicenseExpired:
                case CZ::FinstatApi.FinstatApiException.FailTypeEnum.AccessDisabled:
                    return DialogVariant.Primary;
                default:
                    return DialogVariant.Danger;
            }
        }

        #endregion

        #region Chrome — theme switcher, command palette

        private void InitializeThemeSwitcher()
        {
            // Nothing to switch between while Finstat is the only theme offered — the switcher
            // reappears on its own as soon as a second one is registered.
            comboBoxTheme.IsVisible = _themes.Available.Count > 1;
            comboBoxTheme.ItemsSource = _themes.Available.Select(x => x.Label).ToList();
            var current = _themes.Available
                .Select((theme, index) => new { theme, index })
                .FirstOrDefault(x => x.theme.Id == _themes.Current?.Id);
            comboBoxTheme.SelectedIndex = (current != null) ? current.index : 0;
        }

        private void comboBoxTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = comboBoxTheme.SelectedIndex;
            if (index >= 0 && index < _themes.Available.Count)
            {
                _themes.SetTheme(_themes.Available[index].Id);
            }
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.K && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                commandPalette.IsOpen = true;
                e.Handled = true;
            }
        }

        private void buttonCommands_Click(object sender, RoutedEventArgs e) =>
            commandPalette.IsOpen = true;

        #endregion

        #region Control-Settings

        /// <summary>The settings schema. Birko's <c>Form</c> generates the inputs and two-way binds
        /// each one to the matching <see cref="ViewModel.Settings"/> property.</summary>
        private static IReadOnlyList<FormField> SettingsFields() => new[]
        {
            new FormField { Name = nameof(ViewModel.Settings.PublicKey),      Label = "Public Key",  Hint = "Used by every profile without a key of its own." },
            new FormField { Name = nameof(ViewModel.Settings.PrivateKey),     Label = "Private Key", Type = FieldType.Password },
            new FormField { Name = nameof(ViewModel.Settings.StationName),    Label = "Station Name", Required = true },
            new FormField { Name = nameof(ViewModel.Settings.StationID),      Label = "Station ID",   Required = true },
            new FormField { Name = nameof(ViewModel.Settings.TimeOut),        Label = "TimeOut (ms)", Type = FieldType.Number, Min = 1 },
            new FormField { Name = nameof(ViewModel.Settings.ResponseType),   Label = "Response",     Type = FieldType.Select,
                            Options = Enum.GetValues(typeof(ResponseType)).Cast<object>().ToList() },
            new FormField { Name = nameof(ViewModel.Settings.Lang),           Label = "Language",     Type = FieldType.Select,
                            Options = Enum.GetValues(typeof(ResponseLang)).Cast<object>().ToList() },
        };

        private void LoadSettingsViewModel()
        {
            if (_settingsViewModel == null)
            {
                _settingsViewModel = new ViewModel.Settings();
                settingsForm.Fields = SettingsFields();
                settingsForm.Model = _settingsViewModel;
            }
            _settingsViewModel.FromModel(AppInstance.Settings);
            // Form binds to the model instance, so re-point it to pick the refreshed values up.
            settingsForm.Model = null;
            settingsForm.Model = _settingsViewModel;
            textBlockSettingsErrors.IsVisible = false;
            ShowProfileInSettings(_settingsViewModel.ActiveProfileId);
        }

        private void buttonSettings_Click(object sender, RoutedEventArgs e)
        {
            LoadSettingsViewModel();
            settingsModal.IsOpen = true;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            LoadSettingsViewModel();
            settingsModal.IsOpen = false;
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e) => Save(false);

        private void buttonSaveAndStore_Click(object sender, RoutedEventArgs e) => Save(true);

        private void Save(bool store = false)
        {
            var errors = _settingsViewModel.Validate();
            if (errors.Count == 0)
            {
                AppInstance.Settings = _settingsViewModel.ToModel(AppInstance.Settings);
                if (store)
                {
                    AppInstance.Save();
                }
                RefreshProfileChrome();
                textBlockSettingsErrors.IsVisible = false;
                settingsModal.IsOpen = false;
                return;
            }

            // Inline in the modal rather than a second dialog on top of it.
            var text = new StringBuilder();
            foreach (var kvp in errors)
            {
                foreach (var item in kvp.Value)
                {
                    text.AppendLine(string.Format("{0}: {1}", kvp.Key, item));
                }
            }
            textBlockSettingsErrors.Text = text.ToString().TrimEnd();
            textBlockSettingsErrors.IsVisible = true;
        }

        #endregion
    }
}
