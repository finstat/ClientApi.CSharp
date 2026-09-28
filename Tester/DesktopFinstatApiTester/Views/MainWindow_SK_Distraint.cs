using System;
using System.Linq;
using System.Threading.Tasks;

namespace DesktopFinstatApiTester.Views
{
    public partial class MainWindow
    {
        /// <summary>The two paid endpoints confirm first — they draw down FinStat credit.</summary>
        private const string ChargeWarning = "This method will charge your FinStat credit. Do you want to continue?";

        private async Task SKDistraintSearchRequest()
        {
            if (await GetPromptAsync(new ApiCallParameter(ParameterTypeEnum.Prompt, ChargeWarning)))
            {
                await DoApiRequestAsync("DistraintSearch", "SK", SKDistraintSearch, new[] {
                    new ApiCallParameter(ParameterTypeEnum.String, "IČO", (parameter) => true),
                    new ApiCallParameter(ParameterTypeEnum.String, "Surname", (parameter) => true),
                    new ApiCallParameter(ParameterTypeEnum.String, "Date of Birth", (parameter) => true),
                    new ApiCallParameter(ParameterTypeEnum.String, "City", (parameter) => true),
                    new ApiCallParameter(ParameterTypeEnum.String, "Company Name", (parameter) => true),
                    new ApiCallParameter(ParameterTypeEnum.String, "File Reference", (parameter) => true),
                });
            }
        }

        private object SKDistraintSearch(object[] parameters)
        {
            var client = CreateSKApiDistraintClient();
            var result = client.RequestDistraintSearch((string)parameters[0], (string)parameters[1], (string)parameters[2], (string)parameters[3], (string)parameters[4], (string)parameters[5]).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private async Task SKDistraintDetailRequest()
        {
            if (await GetPromptAsync(new ApiCallParameter(ParameterTypeEnum.Prompt, ChargeWarning)))
            {
                await DoApiRequestAsync("DistraintDetail", "SK", SKDistraintDetail, new[] {
                    new ApiCallParameter(ParameterTypeEnum.String, "Token"),
                    new ApiCallParameter(ParameterTypeEnum.String, "Detail ID List"),
                });
            }
        }

        private object SKDistraintDetail(object[] parameters)
        {
            var client = CreateSKApiDistraintClient();
            var result = client.RequestDistraintDetail((string)parameters[0], ((string)parameters[1]).Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => Int32.Parse(x.Trim())).ToArray()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private Task SKDistraintResultsRequest() => DoApiRequestAsync("DistraintResults", "SK", SKDistraintResults, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "IČO", (parameter) => true),
            new ApiCallParameter(ParameterTypeEnum.String, "Surname", (parameter) => true),
            new ApiCallParameter(ParameterTypeEnum.String, "Date of Birth", (parameter) => true),
            new ApiCallParameter(ParameterTypeEnum.String, "City", (parameter) => true),
            new ApiCallParameter(ParameterTypeEnum.String, "Company Name", (parameter) => true),
            new ApiCallParameter(ParameterTypeEnum.String, "File Reference", (parameter) => true),
        });

        private object SKDistraintResults(object[] parameters)
        {
            var client = CreateSKApiDistraintClient();
            var result = client.RequestDistraintResults((string)parameters[0], (string)parameters[1], (string)parameters[2], (string)parameters[3], (string)parameters[4], (string)parameters[5]).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private Task SKDistraintResultsByTokenRequest() => DoApiRequestAsync("DistraintResultsByToken", "SK", SKDistraintResultsByToken, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "Token"),
        });

        private object SKDistraintResultsByToken(object[] parameters)
        {
            var client = CreateSKApiDistraintClient();
            var result = client.RequestDistraintResultsByToken((string)parameters[0]).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private Task SKDistraintStoredDetailRequest() => DoApiRequestAsync("DistraintStoredDetail", "SK", SKDistraintStoredDetail, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "Detail ID"),
        });

        private object SKDistraintStoredDetail(object[] parameters)
        {
            var client = CreateSKApiDistraintClient();
            var result = client.RequestDistraintStoredDetail((string)parameters[0]).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }
    }
}
