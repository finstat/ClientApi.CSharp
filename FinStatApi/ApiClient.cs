using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinstatApi
{
    public class ApiClient : BaseApiClient
    {
        public ApiClient(string url, string apiKey, string privateKey, string stationId, string stationName, int timeout)
            : base(url, apiKey, privateKey, stationId, stationName, timeout)
        {
        }

        public ApiClient(string apiKey, string privateKey, string stationId, string stationName, int timeout)
            : base(apiKey, privateKey, stationId, stationName, timeout)
        {
        }
        /// <summary>
        /// Requests the beaic for specified ico.
        /// </summary>
        /// <param name="ico">The ico.</param>
        /// <returns>Details</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Specified ico {0} not found in database!
        /// or Url {0} not found!
        /// or Unknown exception while communication with Finstat api!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        public async Task<BasicResult> RequestBasic(string ico, bool json = false)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("ico", ico),
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, ico)),
            });
            return await DoApiCall<BasicResult>("/basic", list, json);
        }

        /// <summary>
        /// Requests the detail for specified ico.
        /// </summary>
        /// <param name="ico">The ico.</param>
        /// <returns>Details</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Specified ico {0} not found in database!
        /// or Url {0} not found!
        /// or Unknown exception while communication with Finstat api!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        public async Task<DetailResult> RequestDetail(string ico, bool json = false)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("ico", ico),
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, ico)),
            });
            return await DoApiCall<DetailResult>("/detail", list, json);
        }
        /// <summary>
        /// Requests the extended detail for specified ico.
        /// </summary>
        /// <param name="ico">The ico.</param>
        /// <returns>Details</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Specified ico {0} not found in database!
        /// or Url {0} not found!
        /// or Unknown exception while communication with Finstat api!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        public async Task<ExtendedResult> RequestExtendedDetail(string ico, bool json = false)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("ico", ico),
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, ico)),
            });
            return await DoApiCall<ExtendedResult>("/extended", list, json);
        }

        /// <summary>
        /// Requests the ultimate detail for specified ico.
        /// </summary>
        /// <param name="ico">The ico.</param>
        /// <returns>Details</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Specified ico {0} not found in database!
        /// or Url {0} not found!
        /// or Unknown exception while communication with Finstat api!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        public async Task<UltimateResult> RequestUltimateDetail(string ico, bool json = false)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("ico", ico),
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, ico)),
            });
            return await DoApiCall<UltimateResult>("/ultimate", list, json);
        }
    }
}
