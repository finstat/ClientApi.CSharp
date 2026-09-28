using System.Threading.Tasks;

namespace DesktopFinstatApiTester.Views
{
    public partial class MainWindow
    {
        private Task SKBasicRequest() => DoApiRequestAsync("Basic", "SK", SKBasic, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "IČO")
        });

        private object SKBasic(object[] parameters)
        {
            var client = CreateSKApiClient();
            var result = client.RequestBasic((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private Task SKDetailRequest() => DoApiRequestAsync("Detail", "SK", SKDetail, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "IČO")
        });

        private object SKDetail(object[] parameters)
        {
            var client = CreateSKApiClient();
            var result = client.RequestDetail((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private Task SKExtendedRequest() => DoApiRequestAsync("Extended", "SK", SKExtended, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "IČO")
        });

        private object SKExtended(object[] parameters)
        {
            var client = CreateSKApiClient();
            var result = client.RequestExtendedDetail((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private Task SKUltimateRequest() => DoApiRequestAsync("Ultimate", "SK", SKUltimate, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "IČO")
        });

        private object SKUltimate(object[] parameters)
        {
            var client = CreateSKApiClient();
            var result = client.RequestUltimateDetail((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }
    }
}
