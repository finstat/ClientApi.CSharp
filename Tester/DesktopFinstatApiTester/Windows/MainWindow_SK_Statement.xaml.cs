using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DesktopFinstatApiTester.Windows
{
    public partial class MainWindow : Fluent.RibbonWindow
    {
        private void buttonStatements_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("StatementList", "SK", SKStatementList, new[] {
                 new ApiCallParameter(ParameterTypeEnum.String, "ICO")
            });
        }

        private object SKStatementList(object[] parameters)
        {
            var client = CreateSKApiStatementClient();
            var result = client.RequestStatements((string)parameters[0]).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonStatementDetail_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("StatementDetail", "SK", SKStatementDetail, new[] {
                 new ApiCallParameter(ParameterTypeEnum.String, "ICO"),
                 new ApiCallParameter(ParameterTypeEnum.Int, "Year"),
                 new ApiCallParameter(ParameterTypeEnum.Pick, "Template") {
                     Values =  new object[]
                     {
                         FinstatApi.Statement.TemplateTypeEnum.Template2011v2,
                         FinstatApi.Statement.TemplateTypeEnum.Template2014,
                         FinstatApi.Statement.TemplateTypeEnum.Template2014micro,
                         FinstatApi.Statement.TemplateTypeEnum.TemplateFinancial,
                         FinstatApi.Statement.TemplateTypeEnum.TemplateNujPU,
                         FinstatApi.Statement.TemplateTypeEnum.TemplateROPO
                     }
                 },
            });
        }

        private object SKStatementDetail(object[] parameters)
        {
            var client = CreateSKApiStatementClient();
            var result = client.RequestStatementDetail((string)parameters[0], (int)parameters[1], (FinstatApi.Statement.TemplateTypeEnum)parameters[2]).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonStatementLegend_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("StatementTemplateLegend", "SK", SKStatementTemplateLegend, new[] {
                new ApiCallParameter(ParameterTypeEnum.Pick, "Template") {
                     Values =  new object[]
                     {
                         FinstatApi.Statement.TemplateTypeEnum.Template2011v2,
                         FinstatApi.Statement.TemplateTypeEnum.Template2014,
                         FinstatApi.Statement.TemplateTypeEnum.Template2014micro,
                         FinstatApi.Statement.TemplateTypeEnum.TemplateFinancial,
                         FinstatApi.Statement.TemplateTypeEnum.TemplateNujPU,
                         FinstatApi.Statement.TemplateTypeEnum.TemplateROPO
                     }
                 },
                new ApiCallParameter(ParameterTypeEnum.Pick, "Language") {
                     Values =  new object[]
                     {
                         "SK",
                         "EN"
                     }
                 }
            });
        }

        private object SKStatementTemplateLegend(object[] parameters)
        {
            var client = CreateSKApiStatementClient();
            var result = client.RequestStatementLegend((FinstatApi.Statement.TemplateTypeEnum)parameters[0], (string)parameters[1]).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }
    }
}
