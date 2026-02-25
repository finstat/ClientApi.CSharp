extern alias CZ;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DesktopFinstatApiTester.Windows
{
    public partial class MainWindow : Fluent.RibbonWindow
    {
        private void buttonCZAutoComplete_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("Autocomplete", "CZ", CZAutoComplete, new[] {
                new ApiCallParameter(ParameterTypeEnum.String, "Text")
            });
        }

        private object CZAutoComplete(object[] parameters)
        {
            var client = CreateCZApiClient();
            var result = client.RequestAutocomplete((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonCZAutoLogin_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("AutoLogIn", "CZ", CZAutoLogin, new[] {
                new ApiCallParameter(ParameterTypeEnum.String, "FinStat URL"),
                new ApiCallParameter(ParameterTypeEnum.String, "Email", (parameter) => true)
            });
        }

        private object CZAutoLogin(object[] parameters)
        {
            var client = CreateCZApiClient();
            var result = client.RequestAutoLogin((string)parameters[0], parameters.Length > 1 ? (string)parameters[1] : null).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }
    }
}
