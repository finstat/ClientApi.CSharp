using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FinstatApi
{
    public class ApiDailyStatement2014DiffClient : AbstractApiClient
    {
        public ApiDailyStatement2014DiffClient(string url, string apiKey, string privateKey, string stationId, string stationName, int timeout)
            : base(url, apiKey, privateKey, stationId, stationName, timeout)
        {
        }

        public ApiDailyStatement2014DiffClient(string apiKey, string privateKey, string stationId, string stationName, int timeout)
            : base(apiKey, privateKey, stationId, stationName, timeout)
        {
        }

        /// <summary>
        /// Requests the list of Statement v2014 DailyDiff files.
        /// </summary>
        /// <returns>List of Statement v2014 DailyDiff files.</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Url {0} not found!
        /// or Timeout exception while communication with Finstat api!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        public async Task<DailyDiffList> RequestListOfDailyStatement2014Diffs(bool json = false)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, null)),
            });
            return await DoApiCall<DailyDiffList>("/GetListOfStatement2014Diffs", list, json);
        }

        /// <summary>
        /// Downloads Statement2014Diff file.
        /// </summary>
        /// <returns>Path to downloaded file.</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Url {0} not found!
        /// or Timeout exception while communication with Finstat api!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        public async Task<string> DownloadDailyStatement2014DiffFile(string fileName, string exportPath)
        {
            try
            {
                var list = new List<KeyValuePair<string, string>>(new[] {
                     new KeyValuePair<string, string>("fileName", fileName),
                     new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, fileName)),
                });
                var responsebytes = await DoApiCall("/GetStatement2014File", list);
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
        /// Requests Legend of statement v2014 files.
        /// </summary>
        /// <returns>List of KeyValue items.</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Url {0} not found!
        /// or Timeout exception while communication with Finstat api!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        public async Task<Statement.StatementLegendResult> RequestStatement2014Legend(string lang = "sk", bool json = false)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("lang", lang),
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, lang)),
            });
            return await DoApiCall<Statement.StatementLegendResult>("/GetStatement2014Legend", list, json);
        }
    }
}
