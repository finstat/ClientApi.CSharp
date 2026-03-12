using System.Text;

namespace FinstatApi
{
    public class PremiumCZResult : DetailResult
    {
        public string VatNumber { get; set; }
        public string TaxPayer { get; set; }
        public BankAccount[] BankAccounts { get; set; }
        public string LegalFormCode { get; set; }
        public string OwnershipCode { get; set; }
        public bool? UnReliability { get; set; }
        public string RegisterNumberText { get; set; }
        public string TradeLicensingOffice { get; set; }
        public int? ActualYear { get; set; }
        public double? SalesActual { get; set; }
        public double? ProfitActual { get; set; }
        public SalesTypeEnum Sales { get; set; }
        public ProfitTypeEnum Profit { get; set; }

        public override string ToString()
        {
            StringBuilder dataString = new StringBuilder();
            dataString.AppendLine(base.ToString());
            dataString.AppendLine(string.Format("VatNumber: {0}", VatNumber));
            dataString.AppendLine(string.Format("TaxPayer: {0}", TaxPayer));
            dataString.AppendLine(string.Format("LegalFormCode: {0}", LegalFormCode));
            dataString.AppendLine(string.Format("OwnershipCode: {0}", OwnershipCode));
            dataString.AppendLine(string.Format("UnReliability: {0}", UnReliability));
            dataString.AppendLine(string.Format("RegisterNumberText: {0}", RegisterNumberText));
            dataString.AppendLine(string.Format("TradeLicensingOffice: {0}", TradeLicensingOffice));
            dataString.AppendLine(string.Format("ActualYear: {0}", ActualYear));
            dataString.AppendLine(string.Format("SalesActual: {0}", SalesActual));
            dataString.AppendLine(string.Format("ProfitActual: {0}", ProfitActual));
            dataString.AppendLine(string.Format("Sales: {0}", Sales));
            dataString.AppendLine(string.Format("Profit: {0}", Profit));
            if (BankAccounts != null && BankAccounts.Length > 0)
            {
                dataString.AppendLine(string.Format("BankAccounts (count): {0}", BankAccounts.Length));
            }
            return dataString.ToString();
        }
    }
}