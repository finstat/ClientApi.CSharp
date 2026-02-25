using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinstatApi
{
    public class ApiDistraintClient : AbstractApiClient
    {
        public ApiDistraintClient(string url, string apiKey, string privateKey, string stationId, string stationName, int timeout)
            : base(url, apiKey, privateKey, stationId, stationName, timeout)
        {
        }

        public ApiDistraintClient(string apiKey, string privateKey, string stationId, string stationName, int timeout)
            : base(apiKey, privateKey, stationId, stationName, timeout)
        {
        }

        /// <summary>
        /// Requests the distraint results for specified search.
        /// </summary>
        /// <param name="ico"></param>
        /// <param name="surname"></param>
        /// <param name="dateOfBirth"></param>
        /// <param name="city"></param>
        /// <param name="companyName"></param>
        /// <param name="fileReference"></param>
        /// <param name="json"></param>
        /// <returns>DistraintResult</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Url {0} not found!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        public async Task<DistraintResult> RequestDistraintSearch(string ico, string surname, string dateOfBirth, string city, string companyName, string fileReference, bool json = false)
        {
            var search = string.Format("{0}|{1}|{2}|{3}|{4}|{5}", ico, surname, dateOfBirth, city, companyName, fileReference);
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("search", search ),
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, search)),
            });
            return await DoApiCall<DistraintResult>("/distraintSearch", list, json);
        }

        /// <summary>
        /// Requests the detail results for specified token and list of detail ids.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="ids"></param>
        /// <param name="json"></param>
        /// <returns>DistraintDetailResult</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Url {0} not found!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        public async Task<DistraintDetailResults> RequestDistraintDetail(string token, int[] ids, bool json = false)
        {
            var idsString = String.Empty;
            var idsParam = String.Empty;
            if (ids != null)
            {
                foreach (var id in ids)
                {
                    idsString += id;
                    idsParam += (!string.IsNullOrEmpty(idsParam) ? "," : null) + id;
                }
            }
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("token", token ),
                new KeyValuePair<string, string>("ids", idsParam ),
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, token + idsString)),
            });
            return await DoApiCall<DistraintDetailResults>("/distraintDetail", list, json);
        }

        /// <summary>
        /// Requests the distraint results for specified search.
        /// </summary>
        /// <param name="ico"></param>
        /// <param name="surname"></param>
        /// <param name="dateOfBirth"></param>
        /// <param name="city"></param>
        /// <param name="companyName"></param>
        /// <param name="fileReference"></param>
        /// <param name="json"></param>
        /// <returns>DistraintResult</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Url {0} not found!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        public async Task<DistraintResult> RequestDistraintResults(string ico, string surname, string dateOfBirth, string city, string companyName, string fileReference, bool json = false)
        {
            var search = string.Format("{0}|{1}|{2}|{3}|{4}|{5}", ico, surname, dateOfBirth, city, companyName, fileReference);
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("search", search ),
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, search)),
            });
            return await DoApiCall<DistraintResult>("/distraintResults", list, json);
        }

        /// <summary>
        /// Requests the distraint results for specified search.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="fileReference"></param>
        /// <param name="json"></param>
        /// <returns>DistraintResult</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Url {0} not found!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        public async Task<DistraintResult> RequestDistraintResultsByToken(string token, bool json = false)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("token", token ),
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, token)),
            });
            return await DoApiCall<DistraintResult>("/distraintResultsByToken", list, json);
        }

        /// <summary>
        /// Requests the stored detail results for specified id.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="ids"></param>
        /// <param name="json"></param>
        /// <returns>DistraintDetailResult</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Url {0} not found!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        public async Task<DistraintDetailResults> RequestDistraintStoredDetail(string id, bool json = false)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("id", id ),
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, id)),
            });
            return await DoApiCall<DistraintDetailResults>("/distraintStoredDetail", list, json);
        }
    }
}
