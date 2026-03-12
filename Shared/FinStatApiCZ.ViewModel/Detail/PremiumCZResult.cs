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
    }
}