using FinstatApi.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FinstatApi
{
    public class ApiDailyStatementDiffClient : AbstractApiClient
    {
        public ApiDailyStatementDiffClient(string url, string apiKey, string privateKey, string stationId, string stationName, int timeout)
            : base(url, apiKey, privateKey, stationId, stationName, timeout)
        {
        }

        public ApiDailyStatementDiffClient(string apiKey, string privateKey, string stationId, string stationName, int timeout)
            : base(apiKey, privateKey, stationId, stationName, timeout)
        {
        }

        /// <summary>
        /// Requests the list of Statement DailyDiff files.
        /// </summary>
        /// <returns>List of Statement DailyDiff files.</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Url {0} not found!
        /// or Timeout exception while communication with Finstat api!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        public async Task<DailyDiffList> RequestListOfDailyStatementDiffs(bool json = false)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, null)),
            });
            return await DoApiCall<DailyDiffList>("/GetListOfStatementDiffs", list, json);
        }

        /// <summary>
        /// Downloads StatementDiff file.
        /// </summary>
        /// <returns>Path to downloaded file.</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Url {0} not found!
        /// or Timeout exception while communication with Finstat api!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        public async Task<string> DownloadDailyStatementDiffFile(string fileName, string exportPath)
        {
            try
            {
                var list = new List<KeyValuePair<string, string>>(new[] {
                     new KeyValuePair<string, string>("fileName", fileName),
                     new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, fileName)),
                });
                var responsebytes = await DoApiCall("/GetStatementFile", list);
                if (responsebytes != null)
                {
                    string fullExportPath = Path.Combine(exportPath, fileName);
                    if (File.Exists(fullExportPath))
                    {
                        File.Delete(fullExportPath);
                    }
                    File.WriteAllBytes(fullExportPath, responsebytes);
                    return fullExportPath;
                }
                return null;
            }
            catch (FinstatApiException e)
            {
                throw e;
            }
            catch (TaskCanceledException e)
            {
                throw new FinstatApiException(FinstatApiException.FailTypeEnum.Timeout, "Timeout exception while processing Finstat api request!", e);
            }
            catch (Exception e)
            {
                throw new FinstatApiException(FinstatApiException.FailTypeEnum.Unknown, "Unknown exception while processing Finstat api request!", e);
            }
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
        public async Task<KeyValue[]> RequestStatementLegend(string lang = "sk", bool json = false)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("lang", lang),
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, lang)),
            });
            return await DoApiCall<KeyValue[]>("/GetStatementLegend", list, json);
        }
    }
}
