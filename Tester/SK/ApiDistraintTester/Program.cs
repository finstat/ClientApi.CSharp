using FinstatApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ApiDistraintTester
{
    public class Program
    {
        public enum LicenceVersionEnum
        {
            ErrorToValidate,
            NotValid,
            Basic,
            Extended,
            Ultimate,
            LimitExceed,
            InsufficientLicense,
            Disabled,
        }

        //private const string ApiUrlConst = "http://localhost.fiddler:3376/api/";
        //private const string ApiUrlConst = "http://localhost:3376/api/";
        //private const string ApiUrlConst = "https://cz.finstat.sk/api/";
        private const string ApiUrlConst = "https://www.finstat.sk/api/";
        private const string TestIcoConst = "35757442";
        private static string _apiKey = null;
        private static string _privateKey = null;
        private static bool _allow_payed_methods = false;

        public static void Main(string[] args)
        {
            /**
             * Example of appsettings.json
             *  {
                  "api_key": "add_api_key",
                  "private_key": "add_private_key"
                }
             */
            var builder = new ConfigurationBuilder()
                     .AddJsonFile("appsettings.json")
                     ;

            var configuration = builder.Build();
            _apiKey = configuration["api_key"];
            _privateKey = configuration["private_key"];
            if (string.IsNullOrEmpty(_apiKey) || _apiKey == "add_api_key")
            {
                Console.Write("api_key missing in .config file, please enter manually: ");
                _apiKey = Console.ReadLine().Trim();
            }
            if (string.IsNullOrEmpty(_privateKey) || _privateKey == "add_private_key")
            {
                Console.Write("private_key missing in .config file, please enter manually: ");
                _privateKey = Console.ReadLine().Trim();
            }

            var apiKeyValidation = CheckApiVersion();
            switch (apiKeyValidation)
            {
                case LicenceVersionEnum.ErrorToValidate:
                    Console.WriteLine("Not able to validate api key, probably no connection to server!!");
                    return;
                case LicenceVersionEnum.NotValid:
                    Console.WriteLine("Not valid api key!!");
                    return;
                case LicenceVersionEnum.Basic:
                    Console.WriteLine("Basic api key detected!!");
                    break;
                case LicenceVersionEnum.Extended:
                    Console.WriteLine("Extended api key detected!!");
                    break;
                case LicenceVersionEnum.Ultimate:
                    Console.WriteLine("Ultimate api key detected!!");
                    break;
                case LicenceVersionEnum.Disabled:
                    Console.WriteLine("You api access is disabled !!");
                    break;
                case LicenceVersionEnum.InsufficientLicense:
                    Console.WriteLine("Your api level is insufficient!!");
                    break;
                case LicenceVersionEnum.LimitExceed:
                    Console.WriteLine("You exceed your api limit!!");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            UrlNotFound();
            FailsWithNotValidCustomerKey();
            TimeoutRequest();
            NotExistingCompany();

            var searchResult = Search(null, "Kocianová", null, "Bratislava", null, null);
            if (searchResult != null && searchResult.Distraints.Length > 0)
            {
                Detail(searchResult.Distraints.FirstOrDefault().DetailToken, searchResult.Distraints.Select(x => x.DetailId).Take(3).ToArray());
            }
            var historyResult = Results(null, "Kocianová2", null, "Bratislava", null, null);
            var historyResultByToken = Results("EB7D559B-F75C-4297-8F16-F9EC3077C6D3");
            var detailResult = StoredDetail("ex-user-erik@myself.sk-131336264713268476-EB7D559B-F75C-4297-8F16-F9EC3077C6D3-173");

            string ident = string.Empty;
            do
            {
                Console.WriteLine();
                Console.Write("Enter ICO,SURNAME,DATE_OF_BIRTH,CITY,COMPANY_NAME,FILE_REFERENCE or just ENTER to exit:");
                ident = Console.ReadLine();
                var idents = ident.Trim(' ').Split(',').Select(x => string.IsNullOrEmpty(x) ? null : x).ToArray();
                if (idents.Length == 6)
                {
                    Results(idents[0], idents[1], idents[2], idents[3], idents[4], idents[5]);
                }
                else
                {
                    Console.WriteLine();
                    Console.Write("Some parameters (ICO, SURNAME, DATE_OF_BIRTH, CITY, COMPANY_NAME,FILE_REFERENCE) are missing!");
                }

            } while (!string.IsNullOrEmpty(ident));
        }

        public static LicenceVersionEnum CheckApiVersion()
        {
            ApiClient apiClient = new ApiClient(ApiUrlConst, _apiKey, _privateKey, "api test", "api test", 60000);
            LicenceVersionEnum result = LicenceVersionEnum.ErrorToValidate;
            bool ischecked = false;
            if (!ischecked)
            {
                apiClient.RequestUltimateDetail(TestIcoConst).ContinueWith(task =>
                {
                    if (!task.IsFaulted && !task.IsCanceled)
                    {
                        result = LicenceVersionEnum.Ultimate;
                        ischecked = true;
                    }
                }).Wait();
            }
            if (!ischecked)
            {
                apiClient.RequestExtendedDetail(TestIcoConst).ContinueWith(task =>
                {
                    if (!task.IsFaulted && !task.IsCanceled)
                    {
                        result = LicenceVersionEnum.Extended;
                        ischecked = true;
                    }
                }).Wait();
            }
            if (!ischecked)
            {
                apiClient.RequestDetail(TestIcoConst).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        var apiException = (FinstatApiException)task.Exception.InnerException;
                        if (apiException.FailType == FinstatApiException.FailTypeEnum.AccessDisabled)
                        {
                            result = LicenceVersionEnum.Disabled;
                        }
                        if (apiException.FailType == FinstatApiException.FailTypeEnum.InsufficientAccess)
                        {
                            result = LicenceVersionEnum.InsufficientLicense;
                        }
                        if (apiException.FailType == FinstatApiException.FailTypeEnum.LimitExceed)
                        {
                            result = LicenceVersionEnum.LimitExceed;
                        }
                        if (apiException.FailType == FinstatApiException.FailTypeEnum.NotValidCustomerKey)
                        {
                            result = LicenceVersionEnum.NotValid;
                        }
                    }
                    else if (task.IsCanceled)
                    {
                        result = LicenceVersionEnum.ErrorToValidate;
                    }
                    else
                    {
                        result = LicenceVersionEnum.Basic;
                        ischecked = true;
                    }
                }).Wait();
            }
            return result;
        }

        /// <summary>
        /// Test pre vyhladavanie exekucie
        /// </summary>
        public static DistraintResult Search(string ico, string surname, string dateOfBirth, string city, string companyName, string fileReference)
        {
            if (!_allow_payed_methods)
            {
                Console.WriteLine("Payed methods are not allowed for testing");
                return null;
            }
            ApiDistraintClient apiClient = new ApiDistraintClient(ApiUrlConst, _apiKey, _privateKey, "api test", "api test", 60000);
            apiClient.RequestDistraintSearch(ico, surname, dateOfBirth, city, companyName, fileReference).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("Load Fails with exception: " + task.Exception.InnerException);
                    return null;
                }
                else if (task.IsCanceled)
                {
                    Console.WriteLine("Result was Canceled");
                    return null;
                }
                else
                {
                    Console.WriteLine("Load OK with values\n {0}", task.Result);
                    return task.Result;
                }
            }).Wait();

            return null;
        }

        /// <summary>
        /// Test pre nacitanie detailu exekucie
        /// </summary>
        public static DistraintDetailResults Detail(string token, int[] ids)
        {
            if (!_allow_payed_methods)
            {
                Console.WriteLine("Payed methods are not allowed for testing");
                return null;
            }
            ApiDistraintClient apiClient = new ApiDistraintClient(ApiUrlConst, _apiKey, _privateKey, "api test", "api test", 60000);
            apiClient.RequestDistraintDetail(token, ids).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("Load Fails with exception: " + task.Exception.InnerException);
                    return null;
                }
                else if (task.IsCanceled)
                {
                    Console.WriteLine("Result was Canceled");
                    return null;
                }
                else
                {
                    Console.WriteLine("Load OK with values\n {0}", task.Result);
                    return task.Result;
                }
            }).Wait();

            return null;
        }

        /// <summary>
        /// Test pre historiu vyhladavani exekucii
        /// </summary>
        public static DistraintResult Results(string ico, string surname, string dateOfBirth, string city, string companyName, string fileReference)
        {
            ApiDistraintClient apiClient = new ApiDistraintClient(ApiUrlConst, _apiKey, _privateKey, "api test", "api test", 60000);
            apiClient.RequestDistraintResults(ico, surname, dateOfBirth, city, companyName, fileReference).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("Load Fails with exception: " + task.Exception.InnerException);
                    return null;
                }
                else if (task.IsCanceled)
                {
                    Console.WriteLine("Result was Canceled");
                    return null;
                }
                else
                {
                    Console.WriteLine("Load OK with values\n {0}", task.Result);
                    return task.Result;
                }
            }).Wait();

            return null;
        }

        /// <summary>
        /// Test pre historiu vyhladavani exekucii
        /// </summary>
        public static DistraintResult Results(string token)
        {
            ApiDistraintClient apiClient = new ApiDistraintClient(ApiUrlConst, _apiKey, _privateKey, "api test", "api test", 60000);
            apiClient.RequestDistraintResultsByToken(token).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("Load Fails with exception: " + task.Exception.InnerException);
                    return null;
                }
                else if (task.IsCanceled)
                {
                    Console.WriteLine("Result was Canceled");
                    return null;
                }
                else
                {
                    Console.WriteLine("Load OK with values\n {0}", task.Result);
                    return task.Result;
                }
            }).Wait();

            return null;
        }

        /// <summary>
        /// Test pre nacitanie ulozeneho detailu exekucie
        /// </summary>
        public static DistraintDetailResults StoredDetail(string id)
        {
            ApiDistraintClient apiClient = new ApiDistraintClient(ApiUrlConst, _apiKey, _privateKey, "api test", "api test", 60000);
            apiClient.RequestDistraintStoredDetail(id).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("Load Fails with exception: " + task.Exception.InnerException);
                    return null;
                }
                else if (task.IsCanceled)
                {
                    Console.WriteLine("Result was Canceled");
                    return null;
                }
                else
                {
                    Console.WriteLine("Load OK with values\n {0}", task.Result);
                    return task.Result;
                }
            }).Wait();

            return null;
        }


        /// <summary>
        /// Test chyby: ak ico nie je vo finstat databaze
        /// </summary>
        public static void NotExistingCompany()
        {
            ApiClient apiClient = new ApiClient(ApiUrlConst, _apiKey, _privateKey, "api test", "api test", 60000);
            apiClient.RequestDetail("a12345678").ContinueWith(task =>
            {
                if (task.IsFaulted && task.Exception != null && task.Exception.InnerException != null && (task.Exception.InnerException is FinstatApiException))
                {
                    FinstatApiException ex = (FinstatApiException)task.Exception.InnerException;
                    if (ex.FailType == FinstatApiException.FailTypeEnum.NotFound)
                    {
                        Console.WriteLine("Not existing company: Test OK!");
                    }
                    else
                    {
                        throw ex;
                    }
                }
                else
                {
                    Console.WriteLine("Not existing company: Test FAIL!");
                }
            }).Wait();
        }

        /// <summary>
        /// Test chyby: request trva dlhsie ako definovany cas
        /// </summary>
        public static void TimeoutRequest()
        {
            ApiClient apiClient = new ApiClient(ApiUrlConst, _apiKey, _privateKey, "api test", "api test", 10);
            apiClient.RequestDetail(TestIcoConst).ContinueWith(task =>
            {
                if (task.IsFaulted && task.Exception != null && task.Exception.InnerException != null && (task.Exception.InnerException is FinstatApiException))
                {
                    FinstatApiException ex = (FinstatApiException)task.Exception.InnerException;
                    if (ex.FailType == FinstatApiException.FailTypeEnum.Timeout)
                    {
                        Console.WriteLine("Request timeout: Test OK!");
                    }
                    else
                    {
                        throw ex;
                    }
                }
                else
                {
                    Console.WriteLine("Request timeout: Test FAIL!");
                }
            }).Wait();
        }

        /// <summary>
        /// Test chyby: naplatny api kluc.
        /// </summary>
        public static void FailsWithNotValidCustomerKey()
        {
            ApiClient apiClient = new ApiClient(ApiUrlConst, "not valid key", "not valid key", "api test", "api test", 60000);
            apiClient.RequestDetail(TestIcoConst).ContinueWith(task =>
            {
                if (task.IsFaulted && task.Exception != null && task.Exception.InnerException != null && (task.Exception.InnerException is FinstatApiException))
                {
                    FinstatApiException ex = (FinstatApiException)task.Exception.InnerException;
                    if (ex.FailType == FinstatApiException.FailTypeEnum.NotValidCustomerKey)
                    {
                        Console.WriteLine("Not valid customer key: Test OK!");
                    }
                    else
                    {
                        throw ex;
                    }
                }
                else
                {
                    Console.WriteLine("Not valid customer key: Test FAIL!");
                }
            }).Wait();
        }

        /// <summary>
        /// Test chyby: ak api url nie je platne alebo nie je mozne sa pripojit
        /// </summary>
        public static void UrlNotFound()
        {
            ApiClient apiClient = new ApiClient("http://not.valid.sk/api", "not valid key", "not valid key", "api test", "api test", 60000);
            apiClient.RequestDetail(TestIcoConst).ContinueWith((task) =>
            {
                if (task.IsFaulted && task.Exception != null && task.Exception.InnerException != null && (task.Exception.InnerException is FinstatApiException))
                {
                    FinstatApiException ex = (FinstatApiException)task.Exception.InnerException;
                    if (ex.FailType == FinstatApiException.FailTypeEnum.UrlNotFound)
                    {
                        Console.WriteLine("Url not found: Test OK!");
                    }
                    else
                    {
                        throw ex;
                    }
                }
                else
                {
                    Console.WriteLine("Url not found: Test FAIL!");
                }
            }).Wait();
        }
    }
}