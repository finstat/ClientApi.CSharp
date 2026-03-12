using System;
using System.Collections.Generic;
using System.Text;

namespace FinstatApi
{
    public class ExtendedResult : BaseResult
    {
        public string[] Phones { get; set; }
        public string[] Emails { get; set; }

        public Debt[] Debts { get; set; }
        public PaymentOrder[] PaymentOrders { get; set; }

        public string EmployeeCode { get; set; }
        public string EmployeeText { get; set; }

        public string OwnershipTypeCode { get; set; }
        public string OwnershipTypeText { get; set; }

        public int? ActualYear { get; set; }

        public double? CreditScoreValue { get; set; }
        public CreditScoreStateEnum? CreditScoreState { get; set; }

        public decimal? BasicCapital { get; set; }
        public double? ProfitPrev { get; set; }
        public double? RevenuePrev { get; set; }

        public double? ForeignResources { get; set; }
        public double? GrossMargin { get; set; }
        public double? ROA { get; set; }

        public DateTime? WarningKaR { get; set; }
        public DateTime? WarningLiquidation { get; set; }

        public bool SelfEmployed { get; set; }
        public Office[] Offices { get; set; }
        public Subject[] Subjects { get; set; }
        public NameParts StructuredName { get; set; }
        public ContactSource[] ContactSources { get; set; }
        public string DisposalUrl { get; set; }
        public bool HasDisposal { get; set; }

        public Ratio[] Ratios { get; set; }
        public JudgementCount[] JudgementCounts { get; set; }
        public DateTime? JudgementLastPublishedDate { get; set; }
        public ReceivableDebt[] StateReceivables { get; set; }
        public ReceivableDebt[] CommercialReceivables { get; set; }

        public double? CreditScoreValueIndex05 { get; set; }
        public CreditScoreStateEnum? CreditScoreStateIndex05 { get; set; }

        public DistraintsAuthorizationInfo DistraintsAuthorization { get; set; }

        public double? CreditScoreValueFinStatScore { get; set; }
        public CreditScoreStateEnum? CreditScoreStateFinStatScore { get; set; }

        public override string ToString()
        {
            StringBuilder dataString = new StringBuilder();
            dataString.AppendLine(base.ToString());
            dataString.AppendLine(string.Format("Ownership: {0} {1}", OwnershipTypeText, OwnershipTypeCode));
            dataString.AppendLine(string.Format("Phones: {0}", string.Join(", ", Phones)));
            dataString.AppendLine(string.Format("Emails: {0}", string.Join(", ", Emails)));
            dataString.AppendLine(string.Format("Employee: {0} {1}", EmployeeText, EmployeeCode));
            dataString.AppendLine(string.Format("ActualYear: {0}", ActualYear));
            dataString.AppendLine(string.Format("BasicCapital: {0}", BasicCapital));
            dataString.AppendLine(string.Format("ProfitPrev: {0}", ProfitPrev));
            dataString.AppendLine(string.Format("RevenuePrev: {0}", RevenuePrev));
            dataString.AppendLine(string.Format("ForeignResources: {0}", ForeignResources));
            dataString.AppendLine(string.Format("GrossMargin: {0}", GrossMargin));
            dataString.AppendLine(string.Format("ROA: {0}", ROA));
            dataString.AppendLine(string.Format("Debts: {0}", Debts == null ? "no debt" : Debt.AsString(Debts)));
            dataString.AppendLine(string.Format("StateReceivables: {0}", StateReceivables == null ? "no state receivables" : Debt.AsString(StateReceivables)));
            dataString.AppendLine(string.Format("CommercialReceivables: {0}", CommercialReceivables == null ? "no commercial receivables" : Debt.AsString(CommercialReceivables)));
            dataString.AppendLine(string.Format("PaymentOrders: {0}", PaymentOrders == null ? "no payment orders" : PaymentOrder.AsString(PaymentOrders)));
            dataString.AppendLine(string.Format("CreditScore: {0}", CreditScoreValue != null ? CreditScoreValue.Value.ToString("0.00") + " " + CreditScoreState : null));
            dataString.AppendLine(string.Format("CreditScoreIndex05: {0}", CreditScoreValueIndex05 != null ? CreditScoreValueIndex05.Value.ToString("0.00") + " " + CreditScoreStateIndex05 : null));
            dataString.AppendLine(string.Format("CreditScoreFinStatScore: {0}", CreditScoreValueFinStatScore != null ? CreditScoreValueFinStatScore.Value.ToString("0.00") + " " + CreditScoreStateFinStatScore : null));
            dataString.AppendLine(string.Format("SelfEmployed: {0}", SelfEmployed));
            dataString.AppendLine(string.Format("DistraintsAuthorizationInfo: {0}", DistraintsAuthorization));

            if (Offices != null)
            {
                dataString.AppendLine("Offices:");
                foreach (Office o in Offices)
                {
                    dataString.AppendLine(string.Format(" - {0}", o));
                }
            }

            if (Subjects != null)
            {
                dataString.AppendLine("Subjects:");
                foreach (Subject s in Subjects)
                {
                    dataString.AppendLine(string.Format(" - {0}", s));
                }
            }
            if (SelfEmployed && (StructuredName != null))
            {
                dataString.AppendLine(string.Format("StructuredName: {0}", StructuredName));
            }

            if (ContactSources != null)
            {
                dataString.AppendLine(string.Format("ContactSources (count): {0}", ContactSources.Length));
            }

            dataString.AppendLine(string.Format("WarningKaR: {0}", WarningKaR));
            dataString.AppendLine(string.Format("HasDisposal: {0}", HasDisposal + " " + DisposalUrl));
            dataString.AppendLine(string.Format("WarningLiquidation: {0}", WarningLiquidation));

            var vals = new List<string>();
            if (JudgementCounts != null && JudgementCounts.Length > 0)
            {
                foreach (var v in JudgementCounts)
                {
                    vals.Add(v.ToString());
                }
            }
            dataString.AppendLine(string.Format("JudgementCounts: [{0}]", string.Join(",", vals.ToArray())));
            dataString.AppendLine(string.Format("JudgementLastPublishedDate: {0}", JudgementLastPublishedDate));
            vals = new List<string>();
            if (Ratios != null && Ratios.Length > 0)
            {
                foreach (var v in Ratios)
                {
                    vals.Add(v.ToString());
                }
            }
            dataString.AppendLine(string.Format("Ratios: [{0}]", string.Join(",", vals.ToArray())));

            return dataString.ToString();
        }

        public class JudgementCount
        {
            public string Name { get; set; }
            public int? Value { get; set; }

            public override string ToString()
            {
                return string.Format("{0}:{1}", Name, Value);
            }
        }

        public class Ratio
        {
            public string Name { get; set; }
            public Item[] Values { get; set; }

            public override string ToString()
            {
                var vals = new List<string>();
                if (Values != null && Values.Length > 0)
                {
                    foreach (var v in Values)
                    {
                        vals.Add(v.ToString());
                    }
                }

                return string.Format("{0}: [{1}]", Name, string.Join(",", vals.ToArray()));
            }

            public class Item
            {
                public int Year { get; set; }
                public double? Value { get; set; }

                public override string ToString()
                {
                    return string.Format("{0}:{1}", Year, Value);
                }
            }
        }

        public class Debt
        {
            public string Source { get; set; }
            public double Value { get; set; }
            public DateTime ValidFrom { get; set; }

            public override string ToString()
            {
                StringBuilder dataString = new StringBuilder();
                dataString.AppendFormat("Source: {0}, ", Source);
                dataString.AppendFormat("Value: {0}, ", Value);
                dataString.AppendFormat("ValidFrom: {0}", ValidFrom);
                return dataString.ToString();
            }

            public static string AsString(Debt[] values)
            {
                StringBuilder dataString = new StringBuilder();
                if (values != null && values.Length > 0)
                {
                    foreach (var value in values)
                    {
                        dataString.AppendLine(value.ToString());
                    }
                }
                return dataString.ToString();
            }
        }

        public class ReceivableDebt : Debt {
            public override string ToString()
            {
                return base.ToString();
            }
        }

        public class PaymentOrder
        {
            public DateTime PublishDate { get; set; }
            public double? Value { get; set; }

            public override string ToString()
            {
                StringBuilder dataString = new StringBuilder();
                dataString.AppendFormat("PublishDate: {0}, ", PublishDate);
                dataString.AppendFormat("Value: {0}", Value);
                return dataString.ToString();
            }

            public static string AsString(PaymentOrder[] values)
            {
                StringBuilder dataString = new StringBuilder();
                if (values != null && values.Length > 0)
                {
                    foreach (var value in values)
                    {
                        dataString.AppendLine(value.ToString());
                    }
                }
                return dataString.ToString();
            }
        }

        public class Office : Address
        {
            public string[] Subjects { get; set; }
            public OfficeType Type { get; set; }

            public override string ToString()
            {
                StringBuilder dataString = new StringBuilder();
                dataString.AppendLine(base.ToString());
                dataString.AppendLine(string.Format("Type: {0}", Type));
                dataString.AppendLine(string.Format("Subjects: {0}", (Subjects != null) ? string.Join(", ", Subjects) : ""));
                return dataString.ToString();
            }
        }

        public enum OfficeType
        {
            Prevadzka
        }

        public class Subject
        {
            public string Title { get; set; }
            public DateTime ValidFrom { get; set; }
            public DateTime? SuspendedFrom { get; set; }
            public DateTime? SuspendedTo { get; set; }

            public override string ToString()
            {
                StringBuilder dataString = new StringBuilder();
                dataString.AppendFormat("Title: {0}, ", Title);
                dataString.AppendFormat("ValidFrom: {0} , ", ValidFrom);
                dataString.AppendFormat("SuspendedFrom: {0}", SuspendedFrom);
                dataString.AppendFormat("SuspendedTo: {0}", SuspendedTo);
                return dataString.ToString();
            }
        }
        public class NameParts
        {
            public string[] Prefix { get; set; }
            public string[] Name { get; set; }
            public string[] Suffix { get; set; }
            public string[] After { get; set; }

            public override string ToString()
            {
                StringBuilder dataString = new StringBuilder();
                dataString.AppendLine(string.Format("Prefix: {0}, ", Prefix != null ? string.Join(" ", Prefix) : null));
                dataString.AppendLine(string.Format("Name: {0}, ", Name != null ? string.Join(" ", Name) : null));
                dataString.AppendLine(string.Format("Suffix: {0}, ", Suffix != null ? string.Join(" ", Suffix) : null));
                dataString.AppendLine(string.Format("After: {0}, ", After != null ? string.Join(" ", After) : null));
                return dataString.ToString();
            }
        }

        public class ContactSource
        {
            public string Contact { get; set; }
            public string[] Sources { get; set; }

            public override string ToString()
            {
                StringBuilder dataString = new StringBuilder();
                dataString.AppendFormat("Contact: {0}, ", Contact);
                dataString.AppendFormat("Sources: {0} , ", Sources != null ? string.Join(", ", Sources) : null);
                return dataString.ToString();
            }
        }

        public class DistraintsAuthorizationInfo
        {
            public DateTime? LastPublishDate { get; set; }
            public int Count { get; set; }

            public override string ToString()
            {
                StringBuilder dataString = new StringBuilder();
                dataString.AppendFormat("LastPublishDate: {0}, ", LastPublishDate);
                dataString.AppendFormat("Count: {0} , ", Count);
                return dataString.ToString();
            }
        }
    }
}
