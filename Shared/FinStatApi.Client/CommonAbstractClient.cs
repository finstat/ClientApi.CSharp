using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace FinstatApi
{
    public delegate void HeadersDelegate(Dictionary<string, string[]> header);
    public delegate void ContentDelegate(byte[] content);

    public class CommonAbstractApiClient
    {
        internal readonly string _url;
        internal readonly string _apiKey;
        internal readonly string _privateKey;
        internal readonly string _stationId;
        internal readonly string _stationName;
        internal readonly int _timeout;

        public Func<HttpClientHandler> HttpClientHandlerFactory { get; set; }

        public ViewModel.Limits Limits { get; protected set; }
        public event HeadersDelegate OnRequest;
        public event HeadersDelegate OnResponse;
        public event ContentDelegate OnErrorResponseContent;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiClient" /> class.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="privateKey">The API private key.</param>
        /// <param name="stationId">The station identifier.</param>
        /// <param name="stationName">Name of the station.</param>
        /// <param name="timeout">The timeout in miliseconds.</param>
        public CommonAbstractApiClient(string url, string apiKey, string privateKey, string stationId, string stationName, int timeout)
        {
            if (!string.IsNullOrEmpty(url) && !url.Contains("localhost"))
            {
                if (url.StartsWith("http://"))
                {
                    url = url.Replace("http://", "https://");
                }
                if (!url.StartsWith("https://"))
                {
                    url = "https://" + url;
                }
            }
            _apiKey = apiKey;
            _privateKey = privateKey;
            _stationId = stationId;
            _stationName = stationName;
            _timeout = timeout;
            _url = url.TrimEnd('/');

            OnResponse += CommonAbstractApiClient_OnResponse;
        }

        private void CommonAbstractApiClient_OnResponse(Dictionary<string, string[]> header)
        {
            Limits = new ViewModel.Limits
            {
                Daily = new ViewModel.Limit
                {
                    Current = (header != null && header.ContainsKey("finstat-daily-limit-current") && header["finstat-daily-limit-current"] != null && header["finstat-daily-limit-current"].Length > 0)
                    ? long.Parse(header["finstat-daily-limit-current"][0]) : 0,
                    Max = (header != null && header.ContainsKey("finstat-daily-limit-max") && header["finstat-daily-limit-max"] != null && header["finstat-daily-limit-max"].Length > 0)
                    ? long.Parse(header["finstat-daily-limit-max"][0]) : 0
                },
                Monthly = new ViewModel.Limit
                {
                    Current = (header != null && header.ContainsKey("finstat-monthly-limit-current") && header["finstat-monthly-limit-current"] != null && header["finstat-monthly-limit-current"].Length > 0)
                    ? long.Parse(header["finstat-monthly-limit-current"][0]) : 0,
                    Max = (header != null && header.ContainsKey("finstat-monthly-limit-max") && header["finstat-monthly-limit-max"] != null && header["finstat-monthly-limit-max"].Length > 0)
                    ? long.Parse(header["finstat-monthly-limit-max"][0]) : 0
                }
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiClient" /> class.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="privateKey">The API private key.</param>
        /// <param name="stationId">The station identifier.</param>
        /// <param name="stationName">Name of the station.</param>
        public CommonAbstractApiClient(string apiKey, string privateKey, string stationId, string stationName, int timeout)
            : this("http://www.finstat.sk/api/", apiKey, privateKey, stationId, stationName, timeout)
        {

        }

        public static string ComputeVerificationHash(string apiKey, string privateKey, string parameter)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(string.Format("SomeSalt+{0}+{1}++{2}+ended", apiKey, privateKey, parameter));
            SHA256Managed hashFunction = new SHA256Managed();
            byte[] hash = hashFunction.ComputeHash(bytes);
            StringBuilder hashString = new StringBuilder();
            foreach (byte x in hash)
            {
                hashString.Append(String.Format("{0:x2}", x));
            }
            return hashString.ToString();
        }

        protected void RaiseOnResponse(Dictionary<string, string[]> header)
        {
            OnResponse?.Invoke(header);
        }

        protected void RaiseOnRequest(Dictionary<string, string[]> header)
        {
            OnRequest?.Invoke(header);
        }

        protected void RaiseOnErrorResponseContent(byte[] content)
        {
            OnErrorResponseContent?.Invoke(content);
        }
    }
}
