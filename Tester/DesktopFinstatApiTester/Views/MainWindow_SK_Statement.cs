using System.Threading.Tasks;

namespace DesktopFinstatApiTester.Views
{
    public partial class MainWindow
    {
        /// <summary>The statement templates the API accepts, offered as a pick list.</summary>
        private static readonly object[] StatementTemplates =
        {
            FinstatApi.Statement.TemplateTypeEnum.Template2011v2,
            FinstatApi.Statement.TemplateTypeEnum.Template2014,
            FinstatApi.Statement.TemplateTypeEnum.Template2014micro,
            FinstatApi.Statement.TemplateTypeEnum.TemplateFinancial,
            FinstatApi.Statement.TemplateTypeEnum.TemplateNujPU,
            FinstatApi.Statement.TemplateTypeEnum.TemplateROPO,
        };

        private Task SKStatementListRequest() => DoApiRequestAsync("StatementList", "SK", SKStatementList, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "ICO")
        });

        private object SKStatementList(object[] parameters)
        {
            var client = CreateSKApiStatementClient();
            var result = client.RequestStatements((string)parameters[0]).GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }

        private Task SKStatementDetailRequest() => DoApiRequestAsync("StatementDetail", "SK", SKStatementDetail, new[] {
            new ApiCallParameter(ParameterTypeEnum.String, "ICO"),
            new ApiCallParameter(ParameterTypeEnum.Int, "Year"),
            new ApiCallParameter(ParameterTypeEnum.Pick, "Template") { Values = StatementTemplates },
        });

        private object SKStatementDetail(object[] parameters)
        {
            var client = CreateSKApiStatementClient();
            var result = client.RequestStatementDetail((string)parameters[0], (int)parameters[1], (FinstatApi.Statement.TemplateTypeEnum)parameters[2]).GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }

        private Task SKStatementTemplateLegendRequest() => DoApiRequestAsync("StatementTemplateLegend", "SK", SKStatementTemplateLegend, new[] {
            new ApiCallParameter(ParameterTypeEnum.Pick, "Template") { Values = StatementTemplates },
            new ApiCallParameter(ParameterTypeEnum.Pick, "Language") { Values = new object[] { "SK", "EN" } },
        });

        private object SKStatementTemplateLegend(object[] parameters)
        {
            var client = CreateSKApiStatementClient();
            var result = client.RequestStatementLegend((FinstatApi.Statement.TemplateTypeEnum)parameters[0], (string)parameters[1]).GetAwaiter().GetResult();
            UpdateLimits(client.Limits);
            return result;
        }
    }
}
