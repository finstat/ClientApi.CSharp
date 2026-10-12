using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesktopFinstatApiTester.Model
{
    public enum ResponseType
    {
        XML,
        JSON
    }

    /// <summary>FinStat response text language (Lang parameter); Default leaves it to the server. Honoured on finstat.cz only.</summary>
    public enum ResponseLang
    {
        Default,
        CS,
        SK
    }
    public class Settings
    {
        public ResponseType ResponseType { get; set; } = ResponseType.XML;
        public ResponseLang Lang { get; set; } = ResponseLang.Default;
        public ApiKeys ApiKeys { get; set; } = new ApiKeys();
        public string StationName { get; set; } = "Api Tester Desktop";
        public string StationID { get; set; } = "api-tester";
        public int TimeOut { get; set; } = 3000;

        public string FinStatApiUrl { get; set; } = "https://www.finstat.sk/api";
        public string FinStatApiUrlCZ { get; set; } = "https://cz.finstat.sk/api";
    }
}
