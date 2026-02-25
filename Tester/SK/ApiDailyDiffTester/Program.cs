using FinstatApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO.Compression;
using System.IO;
using System.Xml.Serialization;
using FinstatApi.ViewModel.Diff;

namespace ApiDailyDiffTester
{
    public class Program
    {
        //private const string ApiUrlConst = "http://ipv4.fiddler:3376/api/";
        //private const string ApiUrlConst = "http://localhost:3376/api/";
        private const string ApiUrlConst = "https://www.finstat.sk/api/";
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

            var diffList = GetListOfDiffs();

            if (diffList != null && diffList.Files.Length > 0)
            {
                string pathToFirstZip = DownloadDiffFile(diffList.Files[0].FileName);
                if (!string.IsNullOrEmpty(pathToFirstZip))
                {
                    var parsedContent = ExtractAndDeserialize(pathToFirstZip);
                    Console.WriteLine("There are " + parsedContent.Length + " company changes in daily diff " + pathToFirstZip + ".");
                    for (int i = 0, count = parsedContent.Length >= 3 ? 3 : parsedContent.Length; i < count; i++)
                    {
                        Console.WriteLine(i + ": " + parsedContent[i]);
                    }
                }
            }
            Console.Write("Press any key to end...");
            Console.ReadKey();

        }

        /// <summary>
        /// Test pre stiahnutie zoznamu diff suborov z daily diff
        /// </summary>
        public static DailyDiffList GetListOfDiffs()
        {
            DailyDiffList result = null;
            ApiDailyDiffClient apiClient = new ApiDailyDiffClient(ApiUrlConst, _apiKey, _privateKey, "api test", "api test", 200000);
            apiClient.RequestListOfDailyDiffs().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("Get current list of diff files fails with exception: " + task.Exception.InnerException);
                }
                else
                {
                    Console.WriteLine("There are " + task.Result.Files.Length + " files in daily diff export.");
                    for (int i = 0, count = task.Result.Files.Length >= 10 ? 10 : task.Result.Files.Length; i < count; i++)
                    {
                        Console.WriteLine(i + ": " + task.Result.Files[i]);
                    }
                    result = task.Result;
                }
            }).Wait();
            return result;
        }

        /// <summary>
        /// Test pre stiahnutie zoznamu diff suborov zavierok z daily diff
        /// </summary>
        public static DailyDiffList GetListOfStatementDiffs()
        {
            DailyDiffList result = null;
            ApiDailyStatementDiffClient apiClient = new ApiDailyStatementDiffClient(ApiUrlConst, _apiKey, _privateKey, "api test", "api test", 200000);
            apiClient.RequestListOfDailyStatementDiffs().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("Get current list of diff files fails with exception: " + task.Exception.InnerException);
                }
                else
                {
                    Console.WriteLine("There are " + task.Result.Files.Length + " files in daily diff export.");
                    for (int i = 0, count = task.Result.Files.Length >= 10 ? 10 : task.Result.Files.Length; i < count; i++)
                    {
                        Console.WriteLine(i + ": " + task.Result.Files[i]);
                    }
                    result = task.Result;
                }
            }).Wait();
            return result;
        }

        /// <summary>
        /// Test pre stiahnutie zoznamu ultimate diff suborov z daily diff
        /// </summary>
        public static DailyDiffList GetListOfUltimateDiffs()
        {
            DailyDiffList result = null;
            ApiDailyUltimateDiffClient apiClient = new ApiDailyUltimateDiffClient(ApiUrlConst, _apiKey, _privateKey, "api test", "api test", 200000);
            apiClient.RequestListOfDailyUltimateDiffs().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("Get current list of diff files fails with exception: " + task.Exception.InnerException);
                }
                else
                {
                    Console.WriteLine("There are " + task.Result.Files.Length + " files in daily diff export.");
                    for (int i = 0, count = task.Result.Files.Length >= 10 ? 10 : task.Result.Files.Length; i < count; i++)
                    {
                        Console.WriteLine(i + ": " + task.Result.Files[i]);
                    }
                    result = task.Result;
                }
            }).Wait();
            return result;
        }

        /// <summary>
        /// Test pre stiahnutie daily diff zip suboru
        /// </summary>
        private static string DownloadDiffFile(string fileName)
        {
            string result = null;
            ApiDailyDiffClient apiClient = new ApiDailyDiffClient(ApiUrlConst, _apiKey, _privateKey, "api test", "api test", 60000);
            apiClient.DownloadDailyDiffFile(fileName, Directory.GetCurrentDirectory()).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("Download file fails with exception: " + task.Exception.InnerException);
                }
                else
                {
                    result = task.Result;
                    if (!string.IsNullOrEmpty(result))
                    {
                        Console.WriteLine("File was succesfully downloaded to {0} with size {1}.", result, new FileInfo(result).Length);
                    }
                }
            }).Wait();
            return result;
        }

        private static string DownloadStatementDiffFile(string fileName)
        {
            string result = null;
            ApiDailyStatementDiffClient apiClient = new ApiDailyStatementDiffClient(ApiUrlConst, _apiKey, _privateKey, "api test", "api test", 60000);
            apiClient.DownloadDailyStatementDiffFile(fileName, Directory.GetCurrentDirectory()).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("Download file fails with exception: " + task.Exception.InnerException);
                }
                else
                {
                    result = task.Result;
                    if (!string.IsNullOrEmpty(result))
                    {
                        Console.WriteLine("File was succesfully downloaded to {0} with size {1}.", result, new FileInfo(result).Length);
                    }
                }
            }).Wait();
            return result;
        }

        private static string DownloadUltimateDiffFile(string fileName)
        {
            string result = null;
            ApiDailyUltimateDiffClient apiClient = new ApiDailyUltimateDiffClient(ApiUrlConst, _apiKey, _privateKey, "api test", "api test", 60000);
            apiClient.DownloadDailyUltimateDiffFile(fileName, Directory.GetCurrentDirectory()).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("Download file fails with exception: " + task.Exception.InnerException);
                }
                else
                {
                    result = task.Result;
                    if (!string.IsNullOrEmpty(result))
                    {
                        Console.WriteLine("File was succesfully downloaded to {0} with size {1}.", result, new FileInfo(result).Length);
                    }
                }
            }).Wait();
            return result;
        }

        /// <summary>
        /// Test pre stiahnutie zoznamu diff suborov zavierok z daily diff
        /// </summary>
        public static FinstatApi.ViewModel.KeyValue[] GetLegendOfStatementDiffs()
        {
            FinstatApi.ViewModel.KeyValue[] result = null;
            ApiDailyStatementDiffClient apiClient = new ApiDailyStatementDiffClient(ApiUrlConst, _apiKey, _privateKey, "api test", "api test", 200000);
            apiClient.RequestStatementLegend().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("Get current list of diff files fails with exception: " + task.Exception.InnerException);
                }
                else
                {
                    result = task.Result;
                    Console.WriteLine("There are " + result.Length + " items in statement legend.");
                    for (int i = 0, count = result.Length; i < count; i++)
                    {
                        Console.WriteLine(result[i]);
                    }
                }
            }).Wait();
            return result;
        }

        private static FinstatApi.ViewModel.Diff.ExtendedResult[] ExtractAndDeserialize(string fileName)
        {
            try
            {
                using (var zip = new ZipArchive(File.OpenRead(fileName), ZipArchiveMode.Read))
                {
                    var enumerator = zip.Entries.GetEnumerator();
                    enumerator.MoveNext();
                    ZipArchiveEntry firstItem = enumerator.Current;
                    XmlSerializer serializer = new XmlSerializer(typeof(FinstatApi.ViewModel.Diff.ExtendedResult[]));
                    return (FinstatApi.ViewModel.Diff.ExtendedResult[])serializer.Deserialize(firstItem.Open());
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Unable to decompress and deserialize with exception: " + exception);
                return null;
            }
        }

        private static FinstatApi.ViewModel.Diff.StatementResult[] ExtractAndDeserializeStatement(string fileName)
        {
            try
            {
                using (var zip = new ZipArchive(File.OpenRead(fileName), ZipArchiveMode.Read))
                {
                    var enumerator = zip.Entries.GetEnumerator();
                    enumerator.MoveNext();
                    ZipArchiveEntry firstItem = enumerator.Current;
                    XmlSerializer serializer = new XmlSerializer(typeof(FinstatApi.ViewModel.Diff.StatementResult[]));
                    return (FinstatApi.ViewModel.Diff.StatementResult[])serializer.Deserialize(firstItem.Open());
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Unable to decompress and deserialize with exception: " + exception);
                return null;
            }
        }

        private static FinstatApi.ViewModel.Diff.UltimateResult[] ExtractAndDeserializeUltimate(string fileName)
        {
            try
            {
                using (var zip = new ZipArchive(File.OpenRead(fileName), ZipArchiveMode.Read))
                {
                    var enumerator = zip.Entries.GetEnumerator();
                    enumerator.MoveNext();
                    ZipArchiveEntry firstItem = enumerator.Current;
                    XmlSerializer serializer = new XmlSerializer(typeof(FinstatApi.ViewModel.Diff.UltimateResult[]));
                    return (FinstatApi.ViewModel.Diff.UltimateResult[])serializer.Deserialize(firstItem.Open());
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Unable to decompress and deserialize with exception: " + exception);
                return null;
            }
        }
    }
}
