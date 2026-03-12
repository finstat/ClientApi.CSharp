using System;
using System.Collections.Generic;
using System.Text;

namespace FinstatApi
{
    public class BaseResult : CommonResult
    {
        public string RegisterNumberText { get; set; }
        public string Dic { get; set; }
        public string IcDPH { get; set; }

        public bool PaymentOrderWarning { get; set; }
        public string PaymentOrderUrl { get; set; }
        public bool OrChange { get; set; }
        public string OrChangeUrl { get; set; }

        public string SkNaceCode { get; set; }
        public string SkNaceText { get; set; }
        public string SkNaceDivision { get; set; }
        public string SkNaceGroup { get; set; }
        public string LegalFormCode { get; set; }
        public string LegalFormText { get; set; }
        public string RpvsInsert { get; set; }
        public string RpvsUrl { get; set; }

        public string SalesCategory { get; set; }
        public IcDphAdditonalData IcDphAdditional { get; set; }
        public double? ProfitActual { get; set; }
        public double? RevenueActual { get; set; }
        public JudgementIndicator[] JudgementIndicators { get; set; }
        public string JudgementFinstatLink { get; set; }

        public bool HasKaR { get; set; }
        public bool HasDebt { get; set; }
        public string KaRUrl { get; set; }
        public string DebtUrl { get; set; }
        public bool Anonymized { get; set; }
        public BankAccount[] BankAccounts { get; set; }
        public string TaxReliabilityIndex { get; set; }
        public DateTime? SuspendedAsPersonUntil { get; set; }

        public override string ToString()
        {
            StringBuilder dataString = new StringBuilder();
            dataString.AppendLine(string.Format("ICO: {0}", Ico));
            dataString.AppendLine(string.Format("Name: {0} in {1}", Name, Activity));
            if (SuspendedAsPersonUntil != null)
            {
                dataString.AppendLine(string.Format("SuspendedAsPersonUntil: {0}", SuspendedAsPersonUntil.Value.ToString("dd.MM.yyyy")));
            }
            dataString.AppendLine(string.Format("LegalForm: {0} {1}", LegalFormCode, LegalFormText));
            dataString.AppendLine(string.Format("Register Number: {0}", RegisterNumberText));
            dataString.AppendLine(string.Format("DIC: {0}", Dic));
            dataString.AppendLine(string.Format("IC DPH: {0}", IcDPH));
            dataString.AppendLine(string.Format("IcDphAdditional: {0}", IcDphAdditional != null ? IcDphAdditional.ToString() : null));
            dataString.AppendLine(string.Format("RpvsInsert: {0} {1}", RpvsInsert, RpvsUrl));
            dataString.AppendLine(string.Format("SalesCategory: {0}", SalesCategory));
            dataString.AppendLine(string.Format("Register Number: {0}", RegisterNumberText));
            dataString.AppendLine(string.Format("SK Nace: {0}", SkNaceCode + "  " + SkNaceText + " " + SkNaceDivision + " " + SkNaceGroup));
            dataString.AppendLine(base.ToString());
            dataString.AppendLine(string.Format("ProfitActual: {0}", ProfitActual));
            dataString.AppendLine(string.Format("RevenueActual: {0}", RevenueActual));
            dataString.AppendLine(string.Format("Warning: {0}", Warning + " " + WarningUrl));
            dataString.AppendLine(string.Format("HasKaR: {0}", HasKaR + " " + KaRUrl));
            dataString.AppendLine(string.Format("HasDebt: {0}", HasDebt + " " + DebtUrl));
            dataString.AppendLine(string.Format("Payment order warning: {0}", PaymentOrderWarning + " " + PaymentOrderUrl));
            dataString.AppendLine(string.Format("OrChange: {0}", OrChange + " " + OrChangeUrl));

            var vals = new List<string>();
            if (JudgementIndicators != null && JudgementIndicators.Length > 0)
            {
                foreach (var v in JudgementIndicators)
                {
                    vals.Add(v.ToString());
                }
            }
            dataString.AppendLine(string.Format("JudgementFinstatLink: {0}", JudgementFinstatLink));
            dataString.AppendLine(string.Format("JudgementIndicators: [{0}]", string.Join(",", vals.ToArray())));

            dataString.AppendLine(string.Format("Anonymized: {0}", Anonymized));

            vals = new List<string>();
            if (BankAccounts != null && BankAccounts.Length > 0)
            {
                foreach (var v in BankAccounts)
                {
                    vals.Add(v.ToString());
                }
            }
            dataString.AppendLine(string.Format("BankAccounts: [{0}]", string.Join(",", vals.ToArray())));
            dataString.AppendLine(string.Format("TaxReliabilityIndex: [{0}]", TaxReliabilityIndex));

            return dataString.ToString();
        }

        public class IcDphAdditonalData
        {
            public string IcDph { get; set; }
            public string Paragraph { get; set; }
            public DateTime? CancelListDetectedDate { get; set; }
            public DateTime? RemoveListDetectedDate { get; set; }
            public override string ToString()
            {
                StringBuilder dataString = new StringBuilder();
                dataString.AppendFormat("IcDph: {0} ", IcDph);
                dataString.AppendFormat("{0}", Paragraph);
                dataString.AppendFormat("{0}", CancelListDetectedDate != null ? "[zoznam s dovodom na zrušenie]" : null);
                dataString.AppendFormat("{0}", RemoveListDetectedDate != null ? "[zoznam vymazaných]" : null);
                return dataString.ToString();
            }
        }

        public class JudgementIndicator
        {
            public string Name { get; set; }
            public bool? Value { get; set; }

            public override string ToString()
            {
                return string.Format("{0}:{1}", Name, Value);
            }
        }
    }
}
