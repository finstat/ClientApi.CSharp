using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FinstatApi;
using Microsoft.Extensions.Configuration;

namespace ApiTesterCore
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
            LoadCompanyDetails(TestIcoConst, apiKeyValidation);

            string ident = string.Empty;
            do
            {
                Console.WriteLine();
                Console.Write("Enter ICO or NAME or just ENTER to exit:");
                ident = Console.ReadLine();
                if (Regex.IsMatch(ident, "^[\\d ]*$"))
                {
                    ident = ident.Replace(" ", string.Empty);
                    LoadCompanyDetails(ident, apiKeyValidation);
                }
                else if (!string.IsNullOrEmpty(ident))
                {
                    var companies = LoadAutoComplete(ident);
                    if (companies != null)
                    {
                        Console.WriteLine();
                        Console.Write("Enter number of company to get details or just ENTER to exit:");
                        ident = Console.ReadLine();
                        int companyIndex;
                        if (int.TryParse(ident, out companyIndex) && companyIndex >= 0 && companyIndex < companies.Length)
                        {
                            LoadCompanyDetails(companies[companyIndex].Ico, apiKeyValidation);
                        }
                    }
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
                    if(!task.IsFaulted && !task.IsCanceled)
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
            return  result;
        }

        /// <summary>
        /// Test pre nacitanie detailu firmy
        /// </summary>
        public static void LoadCompanyDetails(string ident, LicenceVersionEnum version)
        {
            ApiClient apiClient = new ApiClient(ApiUrlConst, _apiKey, _privateKey, "api test", "api test", 60000);
            if (version == LicenceVersionEnum.Ultimate)
            {
                apiClient.RequestUltimateDetail(ident).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Console.WriteLine("Load Fails with exception: " + task.Exception.InnerException);
                    }
                    else if (task.IsCanceled)
                    {
                        Console.WriteLine("Result was Canceled");
                    }
                    else
                    {
                        Console.WriteLine("Load OK with values\n {0}", task.Result);
                    }
                }).Wait();

            }
            else if (version == LicenceVersionEnum.Extended)
            {
                apiClient.RequestExtendedDetail(ident).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Console.WriteLine("Load Fails with exception: " + task.Exception.InnerException);
                    }
                    else if (task.IsCanceled)
                    {
                        Console.WriteLine("Result was Canceled");
                    }
                    else
                    {
                        Console.WriteLine("Load OK with values\n {0}", task.Result);
                    }
                }).Wait();
            }
            else
            {
                apiClient.RequestDetail(ident).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Console.WriteLine("Load Fails with exception: " + task.Exception.InnerException);
                    }
                    else if (task.IsCanceled)
                    {
                        Console.WriteLine("Result was Canceled");
                    }
                    else
                    {
                        Console.WriteLine("Load OK with values\n {0}", task.Result);
                    }
                }).Wait();
            }
        }

        /// <summary>
        /// Test pre nacitanie detailu firmy
        /// </summary>
        public static ApiAutocomplete.Company[] LoadAutoComplete(string query)
        {
            ApiClient apiClient = new ApiClient(ApiUrlConst, _apiKey, _privateKey, "api test", "api test", 60000);
            ApiAutocomplete.Company[] result = null;
            apiClient.RequestAutocomplete(query).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("Load Fails with exception: " + task.Exception.InnerException);
                }
                else if (task.IsCanceled)
                {
                    Console.WriteLine("Result was Canceled");
                }
                else
                {
                    if (task.Result.Results.Length > 0)
                    {
                        for (int i = 0; i < task.Result.Results.Length; i++)
                        {
                            var company = task.Result.Results[i];
                            Console.WriteLine("[{0}] {1}{2}, {3}", i, company.Name, company.Cancelled ? " !zrušená!" : null, company.City);
                        }
                    }
                    else
                    {
                        Console.WriteLine("There is no results found for specified query '{0}'! Try to use some of suggestion below.");
                    }
                    if (task.Result.Suggestions != null && task.Result.Suggestions.Length > 0)
                    {
                        Console.WriteLine("Suggestions: {0}", string.Join(", ", task.Result.Suggestions));
                    }
                    if (task.Result.Results.Length > 0)
                    {
                        result = task.Result.Results;
                    }
                }
            }).Wait();
            return result;
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
