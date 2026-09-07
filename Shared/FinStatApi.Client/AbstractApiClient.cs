using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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

        internal Exception ParseErrorResponse(HttpRequestException e, HttpStatusCode? code, byte[] responseContent, string parameter = null)
        {
            var body = (responseContent != null && responseContent.Length > 0)
                ? Encoding.UTF8.GetString(responseContent)
                : string.Empty;
            var detail = !string.IsNullOrEmpty(body) ? body : e.Message;
            if (code.HasValue)
            {
                switch (code.Value)
                {
                    case HttpStatusCode.Forbidden:
                        if (body.Contains("Insufficient access"))
                        {
                            return new FinstatApiException(FinstatApiException.FailTypeEnum.InsufficientAccess, detail, e);
                        }
                        else if (body.Contains("Your API access and FinStat license expired"))
                        {
                            return new FinstatApiException(FinstatApiException.FailTypeEnum.LicenseExpired, detail, e);
                        }
                        else if (body.Contains("Your API access is disabled"))
                        {
                            return new FinstatApiException(FinstatApiException.FailTypeEnum.AccessDisabled, detail, e);
                        }
                        else if (body.Contains("Invalid verification hash"))
                        {
                            return new FinstatApiException(FinstatApiException.FailTypeEnum.InvalidHash, detail, e);
                        }
                        else
                        {
                            return new FinstatApiException(FinstatApiException.FailTypeEnum.NotValidCustomerKey, detail, e);
                        }
                    case HttpStatusCode.BadRequest:
                        return new FinstatApiException(FinstatApiException.FailTypeEnum.BadRequest, detail, e);
                    case HttpStatusCode.Unauthorized:
                        return new FinstatApiException(FinstatApiException.FailTypeEnum.Unauthorized, detail, e);
                    case HttpStatusCode.PaymentRequired:
                        return new FinstatApiException(FinstatApiException.FailTypeEnum.LimitExceed, detail, e);
                    case HttpStatusCode.NotFound:
                        if (!string.IsNullOrEmpty(parameter))
                        {
                            return new FinstatApiException(FinstatApiException.FailTypeEnum.NotFound, string.Format("Specified ico '{0}' not found in database. Server response: {1}", parameter, detail), e);
                        }
                        else
                        {
                            return new FinstatApiException(FinstatApiException.FailTypeEnum.TooShort, string.Format("Specified query is too short. Server response: {0}", detail), e);
                        }
                    case HttpStatusCode.RequestTimeout:
                        return new FinstatApiException(FinstatApiException.FailTypeEnum.Timeout,
                               string.Format("Request to url {0} timeouts in {1} miliseconds!", _url, _timeout), e);
                    case (HttpStatusCode)451:
                        return new FinstatApiException(FinstatApiException.FailTypeEnum.GdprRestriction, detail, e);
                    case (HttpStatusCode)429:
                        return new FinstatApiException(FinstatApiException.FailTypeEnum.RateLimitExceeded, detail, e);
                    case HttpStatusCode.BadGateway:
                        // An upstream register (currently CRE, behind the live distraint
                        // endpoints) was unreachable. No credit is charged for this, so the
                        // caller may safely retry once the register is back.
                        return new FinstatApiException(FinstatApiException.FailTypeEnum.RegisterUnavailable, detail, e);
                    default:
                        // Keep the server body: it is the only thing that tells the caller
                        // what actually went wrong on an unmapped status code.
                        return new FinstatApiException(FinstatApiException.FailTypeEnum.Unknown,
                               string.Format("Unspecified exception (HTTP {0})! Server response: {1}", (int)code.Value, detail), e);
                }
            }
            else
            {
                if (e.InnerException != null && e.InnerException.Message.Contains("The server name or address could not be resolved"))
                {
                    return new FinstatApiException(FinstatApiException.FailTypeEnum.UrlNotFound, string.Format("Url {0} not found!", _url), e.InnerException);
                }

                return new FinstatApiException(FinstatApiException.FailTypeEnum.OtherCommunicationFail, "Unknown exception while communication with Finstat api!", e);
            }

        }

        internal HttpClient CreateClient(int? timeoutMiliSeconds)
        {
            HttpClient client;
            var handlerFactory = HttpClientHandlerFactory;
            if (handlerFactory != null)
            {
                client = new HttpClient(handlerFactory());
            }
            else
            {
                client = new HttpClient();
            }
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            if (timeoutMiliSeconds.HasValue)
            {
                client.Timeout = new TimeSpan(0, 0, 0, 0, timeoutMiliSeconds.Value);
            }
            return client;
        }

        internal async Task<byte[]> DoApiCall(string methodUrl, List<KeyValuePair<string, string>> methodParams, bool json = false, string method = "POST")
        {
            HttpResponseMessage result = null;
            byte[] resultContent = null;
            try
            {
                var list = new List<KeyValuePair<string, string>>(new[] {
                    new KeyValuePair<string, string>("apiKey", _apiKey),
                    new KeyValuePair<string, string>("StationId", _stationId),
                    new KeyValuePair<string, string>("StationName", _stationName),
                });
                if (methodParams != null && methodParams.Count > 0)
                {
                    list.AddRange(methodParams);
                }
                using (HttpClient client = CreateClient(_timeout))
                {
                    var requestHeaders = new Dictionary<string, string[]>();
                    if (list != null)
                    {
                        foreach (var param in list)
                        {
                            requestHeaders.Add(param.Key, new[] { param.Value });
                        }
                        RaiseOnRequest(requestHeaders);
                    }
                    var content = new FormUrlEncodedContent(list);
                    result = (method == "POST") ? await client.PostAsync(_url + methodUrl + (json ? ".json" : null), content) : await client.GetAsync(_url + methodUrl);
                    resultContent = await result.Content.ReadAsByteArrayAsync();
                    if (result.Headers != null)
                    {
                        // HTTP/2 mandates lowercase header names on the wire, while HTTP/1.1
                        // servers are free to send them capitalised. Key case-insensitively so
                        // the limit headers resolve the same way under either protocol version.
                        var responseHeaders = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
                        foreach (var header in result.Headers)
                        {
                            responseHeaders.Add(header.Key, result.Headers.GetValues(header.Key).ToArray());
                        }
                        RaiseOnResponse(responseHeaders);
                    }
                    result.EnsureSuccessStatusCode();
                    if (result.IsSuccessStatusCode)
                    {
                        return resultContent;
                    }
                    return null;
                }
            }
            catch (HttpRequestException e)
            {
                RaiseOnErrorResponseContent(resultContent);
                throw ParseErrorResponse(e, (result != null) ? result.StatusCode : (HttpStatusCode?)null, resultContent);
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

        internal async Task<T> DoApiCall<T>(string methodUrl, List<KeyValuePair<string, string>> methodParams, bool json = false, string method = "POST")
        {
            try
            {
                var bytes = await DoApiCall(methodUrl, methodParams, json, method);
                if (bytes != null)
                {
                    var response = Encoding.UTF8.GetString(bytes);
                    if (json)
                    {
                        return System.Text.Json.JsonSerializer.Deserialize<T>(response);
                    }
                    else
                    {
                        using (var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(response))))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(T));
                            return (T)serializer.Deserialize(reader);
                        }
                    }
                }
                return default(T);
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
