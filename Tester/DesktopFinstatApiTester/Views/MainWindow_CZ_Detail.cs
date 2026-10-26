extern alias CZ;

using System.Threading.Tasks;

namespace DesktopFinstatApiTester.Views
{
    public partial class MainWindow
    {
        private Task CZBasicRequest() => DoApiRequestAsync("Basic", "CZ", CZBasic, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "IČO"),
            LangParameter()
        });

        private object CZBasic(object[] parameters)
        {
            var client = CreateCZApiClient();
            var result = client.RequestBasic((string)parameters[0], IsJSON(), RequestLang(parameters)).GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }

        private Task CZDetailRequest() => DoApiRequestAsync("Detail", "CZ", CZDetail, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "IČO"),
            LangParameter()
        });

        private object CZDetail(object[] parameters)
        {
            var client = CreateCZApiClient();
            var result = client.RequestDetail((string)parameters[0], IsJSON(), RequestLang(parameters)).GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }

        private Task CZPremiumRequest() => DoApiRequestAsync("PremiumCZ", "CZ", CZPremium, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "IČO"),
            LangParameter()
        });

        private object CZPremium(object[] parameters)
        {
            var client = CreateCZApiClient();
            var result = client.RequestPremium((string)parameters[0], IsJSON(), RequestLang(parameters)).GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }

        private Task CZEliteRequest() => DoApiRequestAsync("EliteCZ", "CZ", CZElite, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "IČO"),
            LangParameter()
        });

        private object CZElite(object[] parameters)
        {
            var client = CreateCZApiClient();
            var result = client.RequestElite((string)parameters[0], IsJSON(), RequestLang(parameters)).GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }

        private Task CZUltimateRequest() => DoApiRequestAsync("UltimateCZ", "CZ", CZUltimate, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "IČO"),
            LangParameter()
        });

        private object CZUltimate(object[] parameters)
        {
            var client = CreateCZApiClient();
            var result = client.RequestUltimate((string)parameters[0], IsJSON(), RequestLang(parameters)).GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }
    }
}
