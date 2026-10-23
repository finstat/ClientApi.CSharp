using System;
using System.Text;
using System.Text.Json.Serialization;

namespace FinstatApi
{
    public class PremiumCZResult : DetailResult
    {
        public string VatNumber { get; set; }
        // VatPayer, IdentifiedPerson or PersonIdentifiedForTax; null when not registered for VAT
        public string TaxPayer { get; set; }
        public BankAccount[] BankAccounts { get; set; }
        public string LegalFormCode { get; set; }
        public string OwnershipCode { get; set; }
        // employee count category code from the latest year that has one
        public string EmployeeCode { get; set; }
        public bool? UnReliability { get; set; }
        public string RegisterNumberText { get; set; }
        public string TradeLicensingOffice { get; set; }
        // PremiumCZ only; EliteCZ/UltimateCZ send the values per year in FinancialIndicators, so these stay null
        public int? ActualYear { get; set; }
        public double? SalesActual { get; set; }
        public double? ProfitActual { get; set; }
        // EliteCZ/UltimateCZ leave Sales and Profit out without financial data, so they read as Unknown
        // EliteCZ/UltimateCZ send them as text, PremiumCZ as numbers; the converter reads both
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SalesTypeEnum Sales { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ProfitTypeEnum Profit { get; set; }
        public TurnoverCategory TurnoverCategory { get; set; }
        public DateTime? SuspendedAsPersonUntil { get; set; }

        public override string ToString()
        {
            StringBuilder dataString = new StringBuilder();
            dataString.AppendLine(base.ToString());
            dataString.AppendLine(string.Format("VatNumber: {0}", VatNumber));
            dataString.AppendLine(string.Format("TaxPayer: {0}", TaxPayer));
            dataString.AppendLine(string.Format("LegalFormCode: {0}", LegalFormCode));
            dataString.AppendLine(string.Format("OwnershipCode: {0}", OwnershipCode));
            dataString.AppendLine(string.Format("EmployeeCode: {0}", EmployeeCode));
            dataString.AppendLine(string.Format("UnReliability: {0}", UnReliability));
            dataString.AppendLine(string.Format("RegisterNumberText: {0}", RegisterNumberText));
            dataString.AppendLine(string.Format("TradeLicensingOffice: {0}", TradeLicensingOffice));
            dataString.AppendLine(string.Format("ActualYear: {0}", ActualYear));
            dataString.AppendLine(string.Format("SalesActual: {0}", SalesActual));
            dataString.AppendLine(string.Format("ProfitActual: {0}", ProfitActual));
            dataString.AppendLine(string.Format("Sales: {0}", Sales));
            dataString.AppendLine(string.Format("Profit: {0}", Profit));
            dataString.AppendLine(string.Format("TurnoverCategory: {0}", TurnoverCategory));
            dataString.AppendLine(string.Format("SuspendedAsPersonUntil: {0}", SuspendedAsPersonUntil));
            if (BankAccounts != null && BankAccounts.Length > 0)
            {
                dataString.AppendLine(string.Format("BankAccounts (count): {0}", BankAccounts.Length));
            }
            return dataString.ToString();
        }
    }

    public class TurnoverCategory
    {
        public int Year { get; set; }
        public string Code { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1} ({2})", Code, Text, Year);
        }
    }
}