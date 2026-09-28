extern alias CZ;

using System.Threading.Tasks;

namespace DesktopFinstatApiTester.Views
{
    public partial class MainWindow
    {
        private Task CZBasicRequest() => DoApiRequestAsync("Basic", "CZ", CZBasic, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "IČO")
        });

        private object CZBasic(object[] parameters)
        {
            var client = CreateCZApiClient();
            var result = client.RequestBasic((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private Task CZDetailRequest() => DoApiRequestAsync("Detail", "CZ", CZDetail, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "IČO")
        });

        private object CZDetail(object[] parameters)
        {
            var client = CreateCZApiClient();
            var result = client.RequestDetail((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private Task CZPremiumRequest() => DoApiRequestAsync("PremiumCZ", "CZ", CZPremium, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "IČO")
        });

        private object CZPremium(object[] parameters)
        {
            var client = CreateCZApiClient();
            var result = client.RequestPremium((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private Task CZEliteRequest() => DoApiRequestAsync("EliteCZ", "CZ", CZElite, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "IČO")
        });

        private object CZElite(object[] parameters)
        {
            var client = CreateCZApiClient();
            var result = client.RequestElite((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }
    }
}
