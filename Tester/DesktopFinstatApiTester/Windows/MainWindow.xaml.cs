extern alias CZ;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace DesktopFinstatApiTester.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Fluent.RibbonWindow
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

        protected ViewModel.ApiApplication AppInstance
        {
            get
            {
                return (Application.Current != null) ? (Application.Current as App).Instance : null;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            DataContext = AppInstance;
            if (String.IsNullOrEmpty(AppInstance?.Settings?.ApiKeys?.PublicKey))
            {
                settingBackStage.IsOpen = true;
            }
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowException(e.Exception);
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        #region Helpers
        public static void Invoke(DispatcherObject control, Action action)
        {
            if (control.Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                control.Dispatcher.Invoke(action);
            }
        }

        public void ShowException(Exception ex, string message = null)
        {
            if (ex != null)
            {
                var exmessage = ex.Message;
                var extitle = "Error";
                if (ex is FinstatApi.FinstatApiException)
                {
                    extitle = "FinStat SK API Exception";
                }
                else if (ex is CZ::FinstatApi.FinstatApiException)
                {
                    var fex = (ex as CZ::FinstatApi.FinstatApiException);
                    extitle = "FinStat CZ API Exception";
                }
                else if (ex is WebException)
                {
                    extitle = "Web Exception";
                }
                else if (ex is TimeoutException)
                {
                    extitle = "Timeout Exception";
                }

                if (MessageBox.Show(((!String.IsNullOrEmpty(message)) ? message : ex.Message) + " Want to see more details?", extitle, MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    var text = ex.Message + "\n-------------\n" + ex.StackTrace;
                    OutputWindow window = new OutputWindow(text)
                    {
                        Owner = this,
                        Title = "Error"
                    };
                    var result = window.ShowDialog();
                }
            }
            else if (ex == null)
            {
                MessageBox.Show("An error occured", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }
        #endregion

        #region Control-Settings
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            LoadSettingsViewModel();
            settingBackStage.IsOpen = false;
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            Save(false);
        }

        private void buttonSaveAndStore_Click(object sender, RoutedEventArgs e)
        {
            Save(true);
        }

        private void Save(bool store = false)
        {
            if (controlSettings.IsValid())
            {
                var viewModel = (ViewModel.Settings)backStageTabItemAccess.DataContext;
                AppInstance.Settings = viewModel.ToModel(AppInstance.Settings);
                if (store)
                {
                    AppInstance.Save();
                }
                settingBackStage.IsOpen = false;
            }
            else
            {
                var errors = controlSettings.GetErrors();
                StringBuilder text = new StringBuilder();
                foreach (var kvp in errors)
                {
                    foreach (var item in kvp.Value)
                    {
                        text.AppendLine(string.Format("{0}: {1}", kvp.Key, item));
                    }
                }
                if (text.Length > 0)
                {
                    MessageBox.Show(text.ToString(), "Form errors", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                }
            }
        }

        private void backStageTabItemAccess_Initialized(object sender, EventArgs e)
        {
            LoadSettingsViewModel();
        }

        private void LoadSettingsViewModel()
        {
            var viewModel = (ViewModel.Settings)backStageTabItemAccess.DataContext;
            if (viewModel == null)
            {
                viewModel = new ViewModel.Settings();
                backStageTabItemAccess.DataContext = viewModel;
            }
            viewModel.FromModel(AppInstance.Settings);
        }
        #endregion
    }
}
