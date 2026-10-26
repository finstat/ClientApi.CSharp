using System.Threading.Tasks;

namespace DesktopFinstatApiTester.Views
{
    public partial class MainWindow
    {
        private Task SKAutoCompleteRequest() => DoApiRequestAsync("Autocomplete", "SK", SKAutoComplete, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "Text")
        });

        private object SKAutoComplete(object[] parameters)
        {
            var client = CreateSKApiClient();
            var result = client.RequestAutocomplete((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }

        private Task SKAutoLoginRequest() => DoApiRequestAsync("AutoLogIn", "SK", SKAutoLogin, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "FinStat URL"),
            new ApiCallParameter(ParameterTypeEnum.String, "Email", (parameter) => true)
        });

        private object SKAutoLogin(object[] parameters)
        {
            var client = CreateSKApiClient();
            var result = client.RequestAutoLogin((string)parameters[0], parameters.Length > 1 ? (string)parameters[1] : null).GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }
    }
}
