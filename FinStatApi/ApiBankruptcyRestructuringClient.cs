using System;
using System.Collections.Generic;

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

        /// <summary>
        /// Requests list of topics of reporting
        /// </summary>
        /// <param name="name">Person name.</param>
        /// <param name="surname">Person surname.</param>
        /// <param name="dateofbirth">Person date of birth.</param>
        /// <returns>List of BankruptcyRestructuring items.</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Url {0} not found!
        /// or Timeout exception while communication with Finstat api!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        public IEnumerable<BankruptcyRestructuring> RequestPersonBankruptcyProceedings(string name, string surname, DateTime dateofbirth, bool json = false)
        {
            System.Collections.Specialized.NameValueCollection reqparm =
            new System.Collections.Specialized.NameValueCollection
            {
                { "name", name},
                { "surName", surname },
                { "dateOfBirth", $"{dateofbirth:yyyy-MM-dd}"},
                { "Hash", ApiClient.ComputeVerificationHash(_apiKey, _privateKey, $"{name}|{surname}|{dateofbirth:yyyy-MM-dd}") },
            };
            return DoApiCall<BankruptcyRestructuring[]>("/PersonBankruptcyProceedings", reqparm, json);
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
        public IEnumerable<BankruptcyRestructuring> RequestCompanyBankruptcyRestructuring(string ico, string name, bool json = false)
        {
            System.Collections.Specialized.NameValueCollection reqparm =
            new System.Collections.Specialized.NameValueCollection
            {
                { "ico", ico},
                { "name", name},
                { "Hash", ApiClient.ComputeVerificationHash(_apiKey, _privateKey, $"{ico}{name}") },
            };
            return DoApiCall<BankruptcyRestructuring[]>("/CompanyBankruptcyRestructuring", reqparm, json);
        }
    }
}
