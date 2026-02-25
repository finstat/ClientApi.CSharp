using FinstatApi;
using Microsoft.Extensions.Configuration;
using System;

namespace ApiCzTesterCore
{
    public class Program
    {
        //private const string ApiUrlConst = "http://ipv4.fiddler:3376/api/";
        //private const string ApiUrlConst = "http://localhost:3376/api/";
        private const string ApiUrlConst = "https://cz.finstat.sk/api/";
        private const string TestIcoConst = "04581806";
        private static string _apiKey = null;
        private static string _privateKey = null;

        public static void Main(string[] args)
        {

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

            FailsWithNotValidCustomerKey();
            AddNotExistingCompany();
            AddToMonitoring(TestIcoConst);
            GetCurrentMonitorings(TestIcoConst);
            GetMonitoringReport();
            RemoveFromMonitoring(TestIcoConst);
            Console.Write("Press any key to end...");
            Console.ReadKey();
        }

        /// <summary>
        /// Test chyby: naplatny api kluc.
        /// </summary>
        public static void FailsWithNotValidCustomerKey()
        {
            ApiMonitoringClient apiClient = new ApiMonitoringClient(ApiUrlConst, "not valid key", "not valid key", "api test", "api test", 60000);
            apiClient.GetMonitorings().ContinueWith(task =>
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
        /// Test chyby: ak ico nie je vo finstat databaze
        /// </summary>
        public static void AddNotExistingCompany()
        {
            ApiMonitoringClient apiClient = new ApiMonitoringClient(ApiUrlConst, _apiKey, _privateKey, "api test", "api test", 60000);
            apiClient.Add("a12345678").ContinueWith(task =>
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
        /// Test pre pridanie do monitoringu
        /// </summary>
        public static void AddToMonitoring(string ico)
        {
            ApiMonitoringClient apiClient = new ApiMonitoringClient(ApiUrlConst, _apiKey, _privateKey, "api test", "api test", 60000);
            apiClient.Add(ico).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("Add to monitoring fails with exception: " + task.Exception.InnerException);
                }
                else
                {
                    Console.WriteLine("Ident " + ico + " added to monitoring with state: " + task.Result);
                }
            }).Wait();
        }

        /// <summary>
        /// Test pre stiahnutie zoznamu firiem v monitoring (s testom na existenciu ico)
        /// </summary>
        public static void GetCurrentMonitorings(string ico)
        {
            ApiMonitoringClient apiClient = new ApiMonitoringClient(ApiUrlConst, _apiKey, _privateKey, "api test", "api test", 60000);
            apiClient.GetMonitorings().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("Get current monitorings fails with exception: " + task.Exception.InnerException);
                }
                else
                {
                    Console.WriteLine("There are " + task.Result.Length + " items in monitoring");
                    if (Array.IndexOf(task.Result, ico) >= 0)
                    {
                        Console.WriteLine("Ico " + ico + " found in current monitoring");
                    }
                    else
                    {
                        Console.WriteLine("Ico " + ico + " NOT found in current monitoring");
                    }
                }
            }).Wait();
        }

        /// <summary>
        /// Test pre stiahnutie zoznamu udalosti pre aktualne firmy v monitoring
        /// </summary>
        public static void GetMonitoringReport()
        {
            ApiMonitoringClient apiClient = new ApiMonitoringClient(ApiUrlConst, _apiKey, _privateKey, "api test", "api test", 60000);
            apiClient.GetReport().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("Get current monitorings fails with exception: " + task.Exception.InnerException);
                }
                else
                {
                    Console.WriteLine("There are " + task.Result.Length + " events in monitoring for last 30 days.");
                    for (int i = 0, count = task.Result.Length >= 10 ? 10 : task.Result.Length; i < count; i++)
                    {
                        Console.WriteLine(i + ": " + task.Result[i]);
                    }
                }
            }).Wait();
        }

        /// <summary>
        /// Test pre odobratie z monitoringu
        /// </summary>
        public static void RemoveFromMonitoring(string ico)
        {
            ApiMonitoringClient apiClient = new ApiMonitoringClient(ApiUrlConst, _apiKey, _privateKey, "api test", "api test", 60000);
            apiClient.Remove(ico).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("Remove from monitoring fails with exception: " + task.Exception.InnerException);
                }
                else
                {
                    Console.WriteLine("Ident " + ico + " removed to monitoring with state: " + task.Result);
                }
            }).Wait();
        }
    }
}