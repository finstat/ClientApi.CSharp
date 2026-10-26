using System;
using System.Threading.Tasks;

namespace DesktopFinstatApiTester.Views
{
    public partial class MainWindow
    {
        private Task SKPersonBankruptcyProceedingsRequest() => DoApiRequestAsync("PersonBankruptcyProceedings", "SK", SKPersonBankruptcyProceedings, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "Name"),
            new ApiCallParameter(ParameterTypeEnum.String, "Surname"),
            new ApiCallParameter(ParameterTypeEnum.Date, "Date of Birth"),
        });

        private object SKPersonBankruptcyProceedings(object[] parameters)
        {
            var client = CreateSKApiBankruptcyRestructuringClient();
            var result = client.RequestPersonBankruptcyProceedings((string)parameters[0], (string)parameters[1], (DateTime)parameters[2], IsJSON()).GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }

        private Task SKCompanyBankruptcyRestructuringRequest() => DoApiRequestAsync("CompanyBankruptcyRestructuring", "SK", SKCompanyBankruptcyRestructuring, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "ICO"),
            new ApiCallParameter(ParameterTypeEnum.String, "Name"),
        });

        private object SKCompanyBankruptcyRestructuring(object[] parameters)
        {
            var client = CreateSKApiBankruptcyRestructuringClient();
            var result = client.RequestCompanyBankruptcyRestructuring((string)parameters[0], (string)parameters[1], IsJSON()).GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }
    }
}
