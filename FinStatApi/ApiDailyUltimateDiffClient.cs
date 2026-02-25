using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FinstatApi
{
    public class ApiDailyUltimateDiffClient : AbstractApiClient
    {
        public ApiDailyUltimateDiffClient(string url, string apiKey, string privateKey, string stationId, string stationName, int timeout)
            : base(url, apiKey, privateKey, stationId, stationName, timeout)
        {
        }

        public ApiDailyUltimateDiffClient(string apiKey, string privateKey, string stationId, string stationName, int timeout)
            : base(apiKey, privateKey, stationId, stationName, timeout)
        {
        }

        /// <summary>
        /// Requests the list of Ultimate DailyDiff files.
        /// </summary>
        /// <returns>List of Ultimate DailyDiff files.</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Url {0} not found!
        /// or Timeout exception while communication with Finstat api!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        public async Task<DailyDiffList> RequestListOfDailyUltimateDiffs(bool json = false)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, null)),
            });
            return await DoApiCall<DailyDiffList>("/GetListOfUltimateDiffs", list, json);
        }

        /// <summary>
        /// Downloads UltimateDiff file.
        /// </summary>
        /// <returns>Path to downloaded file.</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Url {0} not found!
        /// or Timeout exception while communication with Finstat api!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        public async Task<string> DownloadDailyUltimateDiffFile(string fileName, string exportPath)
        {
            try
            {
                var list = new List<KeyValuePair<string, string>>(new[] {
                     new KeyValuePair<string, string>("fileName", fileName),
                     new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, fileName)),
                });
                var responsebytes = await DoApiCall("/GetUltimateFile", list);
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
    }
}
