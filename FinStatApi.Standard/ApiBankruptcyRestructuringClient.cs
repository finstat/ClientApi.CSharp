using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinstatApi
{
    public class ApiBankruptcyRestructuringClient : AbstractApiClient
    {
        public ApiBankruptcyRestructuringClient(string url, string apiKey, string privateKey, string stationId, string stationName, int timeout)
            : base(url, apiKey, privateKey, stationId, stationName, timeout)
        {
        }

        public ApiBankruptcyRestructuringClient(string apiKey, string privateKey, string stationId, string stationName, int timeout)
            : base(apiKey, privateKey, stationId, stationName, timeout)
        {
        }


        public async Task<IEnumerable<BankruptcyRestructuring>> RequestPersonBankruptcyProceedings(string name, string surname, DateTime dateofbirth, bool json = false)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("Name", name),
                new KeyValuePair<string, string>("SurName", surname),
                new KeyValuePair<string, string>("DateOfBirth", $"{dateofbirth:yyyy-MM-dd}"),
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, $"{name}|{surname}|{dateofbirth:yyyy-MM-dd}") ),
            });

            return await DoApiCall<BankruptcyRestructuring[]>("/PersonBankruptcyProceedings", list, json);
        }

        /// <summary>
        /// Requests list of topics of reporting
        /// </summary>
        /// <param name="ico">Company ico.</param>
        /// <param name="name">Company name.</param>
        /// <returns>List of BankruptcyRestructuring items.</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Url {0} not found!
        /// or Timeout exception while communication with Finstat api!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        public async Task<IEnumerable<BankruptcyRestructuring>> RequestCompanyBankruptcyRestructuring(string ico, string name, bool json = false)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("ICO", ico),
                new KeyValuePair<string, string>("Name", name),
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, $"{ico}{name}") ),
            });
            return await DoApiCall<BankruptcyRestructuring[]>("/CompanyBankruptcyRestructuring", list, json);
        }

    }
}
