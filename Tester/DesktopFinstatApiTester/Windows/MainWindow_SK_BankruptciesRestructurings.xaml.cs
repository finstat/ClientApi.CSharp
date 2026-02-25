using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DesktopFinstatApiTester.Windows
{
    public partial class MainWindow : Fluent.RibbonWindow
    {
        #region SK-Person-BAR
        private void buttonPersonBaR_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("PersonBankruptcyProceedings", "SK", SKPersonBankruptcyProceedings, new[] {
                new ApiCallParameter(ParameterTypeEnum.String, "Name"),
                new ApiCallParameter(ParameterTypeEnum.String, "Surname"),
                new ApiCallParameter(ParameterTypeEnum.Date, "Date of Birth"),
            });
        }

        private object SKPersonBankruptcyProceedings(object[] parameters)
        {
            var client = CreateSKApiBankruptcyRestructuringClient();
            var result = client.RequestPersonBankruptcyProceedings((string)parameters[0], (string)parameters[1], (DateTime)parameters[2], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }
        #endregion

        #region SK-Company-BAR
        private void buttonCompanyBaR_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("CompanyBankruptcyRestructuring", "SK", SKCompanyBankruptcyRestructuring, new[] {
                new ApiCallParameter(ParameterTypeEnum.String, "ICO"),
                new ApiCallParameter(ParameterTypeEnum.String, "Name"),
            });
        }

        private object SKCompanyBankruptcyRestructuring(object[] parameters)
        {
            var client = CreateSKApiBankruptcyRestructuringClient();
            var result = client.RequestCompanyBankruptcyRestructuring((string)parameters[0], (string)parameters[1], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }
        #endregion
    }
}
