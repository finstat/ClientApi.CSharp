extern alias CZ;

using System.Threading.Tasks;

namespace DesktopFinstatApiTester.Views
{
    public partial class MainWindow
    {
        private Task CZAutoCompleteRequest() => DoApiRequestAsync("Autocomplete", "CZ", CZAutoComplete, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "Text")
        });

        private object CZAutoComplete(object[] parameters)
        {
            var client = CreateCZApiClient();
            var result = client.RequestAutocomplete((string)parameters[0], IsJSON(), RequestLang()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private Task CZAutoLoginRequest() => DoApiRequestAsync("AutoLogIn", "CZ", CZAutoLogin, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "FinStat URL"),
            new ApiCallParameter(ParameterTypeEnum.String, "Email", (parameter) => true)
        });

        private object CZAutoLogin(object[] parameters)
        {
            var client = CreateCZApiClient();
            var result = client.RequestAutoLogin((string)parameters[0], parameters.Length > 1 ? (string)parameters[1] : null, lang: RequestLang()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }
    }
}
