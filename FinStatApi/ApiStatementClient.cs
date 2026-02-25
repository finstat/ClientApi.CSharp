using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinstatApi
{
    public class ApiStatementClient : AbstractApiClient
    {
        public ApiStatementClient(string url, string apiKey, string privateKey, string stationId, string stationName, int timeout)
            : base(url, apiKey, privateKey, stationId, stationName, timeout)
        {
        }

        public ApiStatementClient(string apiKey, string privateKey, string stationId, string stationName, int timeout)
            : base(apiKey, privateKey, stationId, stationName, timeout)
        {
        }

        /// <summary>
        /// Requests list of statements for given ico
        /// </summary>
        /// <returns>List of StatementItem items.</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Url {0} not found!
        /// or Timeout exception while communication with Finstat api!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        public async Task<Statement.StatementItem[]> RequestStatements(string ico, bool json = false)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("ico", ico),
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, ico)),
            });
            return await DoApiCall<Statement.StatementItem[]>("/GetStatements", list, json);
        }

        // <summary>
        /// Requests statement for given ico, year and template
        /// </summary>
        /// <returns>Statement.StatementResult or Statement.NonProfitStatementResult</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Url {0} not found!
        /// or Timeout exception while communication with Finstat api!
        /// or Unknown exception while communication with Finstat api!
        /// </exceptio
        public async Task<Statement.AbstractStatementResult> RequestStatementDetail(string ico, int year, Statement.TemplateTypeEnum template, bool json = false)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("ico", ico),
                new KeyValuePair<string, string>("year", year.ToString()),
                new KeyValuePair<string, string>("template", template.ToString()),
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, ico + "|" + year)),
            });
            return (new[] { Statement.TemplateTypeEnum.TemplateNujPU, Statement.TemplateTypeEnum.TemplateROPO }.Contains(template))
                 ? (await DoApiCall<Statement.NonProfitStatementResult>("/GetStatementDetail", list, json) as Statement.AbstractStatementResult)
                 : (await DoApiCall<Statement.StatementResult>("/GetStatementDetail", list, json) as Statement.AbstractStatementResult);
        }

        /// <summary>
        /// Requests Legend of statement files.
        /// </summary>
        /// <returns>List of KeyValue items.</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Url {0} not found!
        /// or Timeout exception while communication with Finstat api!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        public async Task<Statement.AbstractStatementLegendResult> RequestStatementLegend(Statement.TemplateTypeEnum template, string lang = "sk", bool json = false)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("lang", lang),
                new KeyValuePair<string, string>("template", template.ToString()),
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, lang)),
            });
            return (template == Statement.TemplateTypeEnum.TemplateNujPU || template == Statement.TemplateTypeEnum.TemplateROPO)
                ? (await DoApiCall<Statement.NonProfitStatementLegendResult>("/GetStatementTemplateLegend", list, json) as Statement.AbstractStatementLegendResult)
                : (await DoApiCall<Statement.StatementLegendResult>("/GetStatementTemplateLegend", list, json) as Statement.AbstractStatementLegendResult);
        }
    }
}
