using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FinstatApi
{
    public class ApiReportingClient : AbstractApiClient
    {
        public ApiReportingClient(string url, string apiKey, string privateKey, string stationId, string stationName, int timeout)
            : base(url, apiKey, privateKey, stationId, stationName, timeout)
        {
        }

        public ApiReportingClient(string apiKey, string privateKey, string stationId, string stationName, int timeout)
            : base(apiKey, privateKey, stationId, stationName, timeout)
        {
        }

        /// <summary>
        /// Requests list of topics of reporting
        /// </summary>
        /// <returns>List of ReportingTopic items.</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Url {0} not found!
        /// or Timeout exception while communication with Finstat api!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        public async Task<Reporting.ReportingTopic[]> RequestTopics(bool json = false)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("Hash", ApiClient.ComputeVerificationHash(_apiKey, _privateKey, "reporting-topics")),
            });

            return await DoApiCall<Reporting.ReportingTopic[]>("/GetReportingTopics", list, json);
        }

        // <summary>
        /// Requests list of Generated user reporting outputs for given topic
        /// </summary>
        /// <returns>List of ReportOutput</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Url {0} not found!
        /// or Timeout exception while communication with Finstat api!
        /// or Unknown exception while communication with Finstat api!
        /// </exceptio
        public async Task<Reporting.ReportOutput[]> RequestList(string topic, bool json = false)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("topic", topic),
                new KeyValuePair<string, string>("Hash", ApiClient.ComputeVerificationHash(_apiKey, _privateKey, "reporting-list|" + topic)),
            });
            return await DoApiCall<Reporting.ReportOutput[]>("/GetReportingList", list, json);
        }


        /// <summary>
        /// Downloads reporting excel File .
        /// </summary>
        /// <returns>Reporting output file.</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Url {0} not found!
        /// or Timeout exception while communication with Finstat api!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        public async Task<string> DownloadReportFile(string fileName, string exportPath)
        {
            try
            {
                var list = new List<KeyValuePair<string, string>>(new[] {
                     new KeyValuePair<string, string>("FileName", fileName ),
                     new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, fileName)),
                });
                var responsebytes = await DoApiCall("/GetReportingOutput", list);
                if (responsebytes != null)
                {
                    string fullExportPath = Path.Combine(exportPath, fileName + ".xlsx");
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
