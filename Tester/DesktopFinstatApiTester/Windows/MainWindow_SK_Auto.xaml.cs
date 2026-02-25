using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DesktopFinstatApiTester.Windows
{
    public partial class MainWindow : Fluent.RibbonWindow
    {
        #region SK-Auto
        private void buttonAutoComplete_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("Autocomplete", "SK", SKAutoComplete, new[] {
                new ApiCallParameter(ParameterTypeEnum.String, "Text")
            });
        }

        private object SKAutoComplete(object[] parameters)
        {
            var client = CreateSKApiClient();
            var result = client.RequestAutocomplete((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonAutoLogin_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("AutoLogIn", "SK", SKAutoLogin, new[] {
                new ApiCallParameter(ParameterTypeEnum.String, "FinStat URL"),
                new ApiCallParameter(ParameterTypeEnum.String, "Email", (parameter) => true)
            });
        }

        private object SKAutoLogin(object[] parameters)
        {
            var client = CreateSKApiClient();
            var result = client.RequestAutoLogin((string)parameters[0], parameters.Length > 1 ? (string)parameters[1] : null).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }
        #endregion
    }
}
