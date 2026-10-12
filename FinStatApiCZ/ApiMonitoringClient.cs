using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinstatApi
{
    public class ApiMonitoringClient : AbstractApiClient
    {

        public ApiMonitoringClient(string url, string apiKey, string privateKey, string stationId, string stationName, int timeout)
            : base(url, apiKey, privateKey, stationId, stationName, timeout)
        {
        }

        public ApiMonitoringClient(string apiKey, string privateKey, string stationId, string stationName, int timeout)
            : base(apiKey, privateKey, stationId, stationName, timeout)
        {
        }

        /// <summary>
        /// Adds specified ico to monitoring.
        /// </summary>
        /// <param name="ico">The ico.</param>
        /// <returns>True if succeed otherwise false.</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Specified ico {0} not found in database!
        /// or Url {0} not found!
        /// or TimeOut exception while communication with Finstat api!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        public async Task<bool> Add(string ico, bool json = false, string lang = null)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("ico", ico),
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, ico)),
            });
            return await DoApiCall<bool>("/AddToMonitoring", list, json, lang: lang);
        }

        public async Task<bool> Add(string ico, string category, bool json = false, string lang = null)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("ico", ico),
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, ico)),
            });
            if (!string.IsNullOrEmpty(category))
                list.Add(new KeyValuePair<string, string>("category", category));
            return await DoApiCall<bool>("/AddToMonitoring", list, json, lang: lang);
        }

        /// <summary>
        /// Removes specified ico from monitoring.
        /// </summary>
        /// <param name="ico">The ico.</param>
        /// <returns>True if succeed otherwise false.</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Specified ico {0} not found in database!
        /// or Url {0} not found!
        /// or TimeOut exception while communication with Finstat api!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        public async Task<bool> Remove(string ico, bool json = false, string lang = null)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("ico", ico),
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, ico)),
            });
            return await DoApiCall<bool>("/RemoveFromMonitoring", list, json, lang: lang);
        }

        public async Task<bool> Remove(string ico, string category, bool json = false, string lang = null)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("ico", ico),
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, ico)),
            });
            if (!string.IsNullOrEmpty(category))
                list.Add(new KeyValuePair<string, string>("category", category));
            return await DoApiCall<bool>("/RemoveFromMonitoring", list, json, lang: lang);
        }

        /// <summary>
        /// Retrieves list of current monitorings.
        /// </summary>
        /// <returns>List of monitored ICO's.</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Url {0} not found!
        /// or TimeOut exception while communication with Finstat api!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        public async Task<string[]> GetMonitorings(bool json = false, string lang = null)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, "list")),
            });
            return await DoApiCall<string[]>("/MonitoringList", list, json, lang: lang);
        }

        public async Task<string[]> GetMonitorings(string category, bool json = false, string lang = null)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, "list")),
            });
            if (!string.IsNullOrEmpty(category))
                list.Add(new KeyValuePair<string, string>("category", category));
            return await DoApiCall<string[]>("/MonitoringList", list, json, lang: lang);
        }

        /// <summary>
        /// Retrieves report of events in current monitorings.
        /// </summary>
        /// <returns>List of monitoring events.</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Url {0} not found!
        /// or TimeOut exception while communication with Finstat api!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        public async Task<Monitoring[]> GetReport(bool json = false, string lang = null)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, "report")),
            });
            return await DoApiCall<Monitoring[]>("/MonitoringReport", list, json, lang: lang);
        }

        public async Task<Monitoring[]> GetReport(string category, bool json = false, string lang = null)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, "report")),
            });
            if (!string.IsNullOrEmpty(category))
                list.Add(new KeyValuePair<string, string>("category", category));
            return await DoApiCall<Monitoring[]>("/MonitoringReport", list, json, lang: lang);
        }

        /// <summary>
        /// Retrieves list of user monitoring categories
        /// </summary>
        /// <returns>List of user monitoring categories.</returns>
        /// <exception cref="FinstatApi.FinstatApiException">
        /// Not valid API key!
        /// or Url {0} not found!
        /// or TimeOut exception while communication with Finstat api!
        /// or Unknown exception while communication with Finstat api!
        /// </exception>
        ///
        public async Task<MonitoringCategory[]> GetCategories(bool json, string lang = null)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, "monitoringcategories")),
            });
            return await DoApiCall<MonitoringCategory[]>("/MonitoringCategories", list, json, lang: lang);
        }

        /// <summary>
        /// Adds specified date of birth to monitoring.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>True if succeed otherwise false.</returns>
        public async Task<bool> AddDate(string date, bool json = false, string lang = null)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("date", date),
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, date)),
            });
            return await DoApiCall<bool>("/AddDateToMonitoring", list, json, lang: lang);
        }

        public async Task<bool> AddDate(string date, string category, bool json = false, string lang = null)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("date", date),
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, date)),
            });
            if (!string.IsNullOrEmpty(category))
                list.Add(new KeyValuePair<string, string>("category", category));
            return await DoApiCall<bool>("/AddDateToMonitoring", list, json, lang: lang);
        }

        /// <summary>
        /// Removes specified date of birth from monitoring.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>True if succeed otherwise false.</returns>
        public async Task<bool> RemoveDate(string date, bool json = false, string lang = null)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("date", date),
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, date)),
            });
            return await DoApiCall<bool>("/RemoveDateFromMonitoring", list, json, lang: lang);
        }

        public async Task<bool> RemoveDate(string date, string category, bool json = false, string lang = null)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("date", date),
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, date)),
            });
            if (!string.IsNullOrEmpty(category))
                list.Add(new KeyValuePair<string, string>("category", category));
            return await DoApiCall<bool>("/RemoveDateFromMonitoring", list, json, lang: lang);
        }

        /// <summary>
        /// Retrieves list of currently monitored dates of birth.
        /// </summary>
        public async Task<string[]> GetDateMonitorings(bool json = false, string lang = null)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, "datelist")),
            });
            return await DoApiCall<string[]>("/MonitoringDateList", list, json, lang: lang);
        }

        public async Task<string[]> GetDateMonitorings(string category, bool json = false, string lang = null)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, "datelist")),
            });
            if (!string.IsNullOrEmpty(category))
                list.Add(new KeyValuePair<string, string>("category", category));
            return await DoApiCall<string[]>("/MonitoringDateList", list, json, lang: lang);
        }

        /// <summary>
        /// Retrieves report of date-of-birth events in current monitoring.
        /// </summary>
        public async Task<MonitoringDate[]> GetDateReport(bool json = false, string lang = null)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, "datereport")),
            });
            return await DoApiCall<MonitoringDate[]>("/MonitoringDateReport", list, json, lang: lang);
        }

        public async Task<MonitoringDate[]> GetDateReport(string category, bool json = false, string lang = null)
        {
            var list = new List<KeyValuePair<string, string>>(new[] {
                new KeyValuePair<string, string>("Hash", ComputeVerificationHash(_apiKey, _privateKey, "datereport")),
            });
            if (!string.IsNullOrEmpty(category))
                list.Add(new KeyValuePair<string, string>("category", category));
            return await DoApiCall<MonitoringDate[]>("/MonitoringDateReport", list, json, lang: lang);
        }
    }
}
