using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Serialization;

namespace FinstatApi
{
    public class AbstractApiClient : CommonAbstractApiClient
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractApiClient" /> class.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="privateKey">The API private key.</param>
        /// <param name="stationId">The station identifier.</param>
        /// <param name="stationName">Name of the station.</param>
        /// <param name="timeout">The timeout in miliseconds.</param>
        public AbstractApiClient(string url, string apiKey, string privateKey, string stationId, string stationName, int timeout)
            : base(url, apiKey, privateKey, stationId, stationName, timeout)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractApiClient" /> class.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="privateKey">The API private key.</param>
        /// <param name="stationId">The station identifier.</param>
        /// <param name="stationName">Name of the station.</param>
        public AbstractApiClient(string apiKey, string privateKey, string stationId, string stationName, int timeout)
            : this("http://www.finstat.sk/api/", apiKey, privateKey, stationId, stationName, timeout)
        {

        }

        internal Exception ParseErrorResponse(WebException e, string parameter = null)
        {
            if (e.Response is HttpWebResponse)
            {
                ParseWebResponse(e, parameter);
            }
            else if (e.Status == WebExceptionStatus.ConnectFailure || e.Status == WebExceptionStatus.NameResolutionFailure)
            {
                return new FinstatApiException(FinstatApiException.FailTypeEnum.UrlNotFound,
                            string.Format("Url {0} not found!", _url), e);
            }
            else if (e.Status == WebExceptionStatus.Timeout)
            {
                return new FinstatApiException(FinstatApiException.FailTypeEnum.Timeout,
                            string.Format("Request to url {0} timeouts in {1} miliseconds!", _url, _timeout), e);
            }
            return new FinstatApiException(FinstatApiException.FailTypeEnum.OtherCommunicationFail, "Unknown exception while communication with Finstat api!", e);
        }

        internal static void ParseWebResponse(WebException e, string parameter = null)
        {
            HttpWebResponse response = (HttpWebResponse)e.Response;
            switch (response.StatusCode)
            {
                case HttpStatusCode.Forbidden:
                    if (response.StatusDescription.StartsWith("Insufficient access"))
                    {
                        throw new FinstatApiException(FinstatApiException.FailTypeEnum.InsufficientAccess, response.StatusDescription, e);
                    }
                    else if (response.StatusDescription.StartsWith("Your API access and Finstat license expired"))
                    {
                        throw new FinstatApiException(FinstatApiException.FailTypeEnum.LicenseExpired, response.StatusDescription, e);
                    }
                    else if (response.StatusDescription.StartsWith("Your API access is disabled"))
                    {
                        throw new FinstatApiException(FinstatApiException.FailTypeEnum.AccessDisabled, response.StatusDescription, e);
                    }
                    else if (response.StatusDescription.StartsWith("Invalid verification hash"))
                    {
                        throw new FinstatApiException(FinstatApiException.FailTypeEnum.InvalidHash, response.StatusDescription, e);
                    }
                    else
                    {
                        throw new FinstatApiException(FinstatApiException.FailTypeEnum.NotValidCustomerKey, response.StatusDescription, e);
                    }
                case HttpStatusCode.BadRequest:
                    throw new FinstatApiException(FinstatApiException.FailTypeEnum.BadRequest, response.StatusDescription, e);
                case HttpStatusCode.PaymentRequired:
                    throw new FinstatApiException(FinstatApiException.FailTypeEnum.LimitExceed, response.StatusDescription, e);
                case HttpStatusCode.NotFound:
                    if (!string.IsNullOrEmpty(parameter))
                    {
                        throw new FinstatApiException(FinstatApiException.FailTypeEnum.NotFound, string.Format("Specified ico '{0}' not found in database. Server response: {1}", parameter, e.Message), e);
                    }
                    else
                    {
                        throw new FinstatApiException(FinstatApiException.FailTypeEnum.TooShort, string.Format("Specified parameters are not valid. Server response: {0}", e.Message), e);
                    }
                default:
                    throw new FinstatApiException(FinstatApiException.FailTypeEnum.Unknown, "Unspecified exception!", e);
            }
        }

        internal byte[] DoApiCall(string methodUrl, System.Collections.Specialized.NameValueCollection methodParams, bool json = false, string method = "POST")
        {
            try
            {
                System.Collections.Specialized.NameValueCollection reqparm =
                new System.Collections.Specialized.NameValueCollection
                {
                    { "apiKey", _apiKey },
                    { "StationId", _stationId },
                    { "StationName", _stationName }
                };
                if (methodParams != null && methodParams.Count > 0)
                {
                    reqparm.Add(methodParams);
                }

                X509Certificate2 cert = null;
#if DEBUG
                if (_url.Contains("zdrojak.eu"))
                {
                    X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                    store.Open(OpenFlags.ReadOnly);
                    X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindByIssuerName, "root_ca_dev_zdrojak.eu", false);
                    if (certs.Count > 0)
                    {
                        cert = certs[0];
                    }
                    else
                    {
                        X509Store storeMachine = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                        storeMachine.Open(OpenFlags.ReadOnly);
                        X509Certificate2Collection certsMachine = store.Certificates.Find(X509FindType.FindByIssuerName, "root_ca_dev_zdrojak.eu", false);
                        if (certsMachine.Count > 0)
                        {
                            cert = certsMachine[0];
                        }
                    }
                }
#endif
                using (WebClient client = new WebClientWithTimeout(_timeout, cert))
                {
                    var requestHeaders = new Dictionary<string, string[]>();
                    if (reqparm != null)
                    {
                        foreach (var param in reqparm.AllKeys)
                        {
                            requestHeaders.Add(param, reqparm.GetValues(param));
                        }
                        RaiseOnRequest(requestHeaders);
                    }

                    var result = client.UploadValues(_url + methodUrl + (json ? ".json" : null), method, reqparm);
                    if (client.ResponseHeaders != null)
                    {
                        var responseHeaders = new Dictionary<string, string[]>();
                        foreach (var headerKey in client.ResponseHeaders.AllKeys)
                        {
                            responseHeaders.Add(headerKey, client.ResponseHeaders.GetValues(headerKey));
                        }
                        RaiseOnResponse(responseHeaders);
                    }

                    return result;
                }
            }
            catch (WebException e)
            {
                var resp = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                RaiseOnErrorResponseContent(Encoding.UTF8.GetBytes(resp));
                var responseHeaders = new Dictionary<string, string[]>();
                foreach (var headerKey in e.Response.Headers.AllKeys)
                {
                    responseHeaders.Add(headerKey, e.Response.Headers.GetValues(headerKey));
                }
                RaiseOnResponse(responseHeaders);
                throw ParseErrorResponse(e);
            }
            catch (Exception e)
            {
                throw new FinstatApiException(FinstatApiException.FailTypeEnum.Unknown, "Unknown exception while processing Finstat api request!", e);
            }
        }

        internal T DoApiCall<T>(string methodUrl, System.Collections.Specialized.NameValueCollection methodParams, bool json = false, string method = "POST")
        {
            try
            {
                byte[] responsebytes = DoApiCall(methodUrl, methodParams, json, method);
                var response = Encoding.UTF8.GetString(responsebytes);

                using (var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(response))))
                {
                    if (json)
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        return (T)serializer.Deserialize(reader, typeof(T));
                    }
                    else
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(T));
                        return (T)serializer.Deserialize(reader);
                    }
                }
            }
            catch (FinstatApiException e)
            {
                throw e;
            }
            catch (WebException e)
            {
                var responseHeaders = new Dictionary<string, string[]>();
                foreach (var headerKey in e.Response.Headers.AllKeys)
                {
                    responseHeaders.Add(headerKey, e.Response.Headers.GetValues(headerKey));
                }
                RaiseOnResponse(responseHeaders);
                throw ParseErrorResponse(e);
            }
            catch (Exception e)
            {
                throw new FinstatApiException(FinstatApiException.FailTypeEnum.Unknown, "Unknown exception while processing Finstat api request!", e);
            }
        }
    }
}
