using System;
using System.Text;

namespace FinstatApi
{
    public class UltimateCZResult : EliteCZResult
    {
        public ORCRPerson[] ORCRPersons { get; set; }
        public RZPPerson[] RZPPersons { get; set; }
        public string StatutoryAction { get; set; }
        public string ProcurationAction { get; set; }
        // active trade licences for RŽP subjects, active ORČR business subjects otherwise
        public Subject[] Subjects { get; set; }
        // 4 years of history as FinancialIndicators
        public CreditScore[] CreditScores { get; set; }
        public FiledInsolvencyClaims FiledInsolvencyClaims { get; set; }

        public override string ToString()
        {
            StringBuilder dataString = new StringBuilder();
            dataString.AppendLine(base.ToString());
            AppendItems(dataString, "ORCRPersons", ORCRPersons);
            AppendItems(dataString, "RZPPersons", RZPPersons);
            dataString.AppendLine(string.Format("StatutoryAction: {0}", StatutoryAction));
            dataString.AppendLine(string.Format("ProcurationAction: {0}", ProcurationAction));
            AppendItems(dataString, "Subjects", Subjects);
            AppendItems(dataString, "CreditScores", CreditScores);
            if (FiledInsolvencyClaims != null)
            {
                dataString.AppendLine("FiledInsolvencyClaims:");
                dataString.AppendLine(FiledInsolvencyClaims.ToString());
            }
            return dataString.ToString();
        }
    }

    /// <summary>
    /// Claims filed by the company as creditor in active insolvency proceedings (at most 300 latest proceedings).
    /// </summary>
    public class FiledInsolvencyClaims
    {
        public int ProceedingsCount { get; set; }
        // null when no application was scanned, so unknown is not reported as 0
        public decimal? AmountOfClaims { get; set; }
        public FiledClaimApplication[] Applications { get; set; }

        public override string ToString()
        {
            StringBuilder dataString = new StringBuilder();
            dataString.AppendLine(string.Format("ProceedingsCount: {0}", ProceedingsCount));
            dataString.AppendLine(string.Format("AmountOfClaims: {0}", AmountOfClaims));
            EliteCZResult.AppendItems(dataString, "Applications", Applications);
            return dataString.ToString();
        }
    }

    public class FiledClaimApplication
    {
        public string FileReference { get; set; }
        public string Url { get; set; }
        public IsirParty[] Debtors { get; set; }
        public IsirParty Representative { get; set; }
        public DateTime PublishDate { get; set; }
        public int? ClaimsCount { get; set; }
        public decimal? TotalAmount { get; set; }

        public override string ToString()
        {
            return string.Format("FileReference: {0}, Debtors: {1}, Representative: {2}, PublishDate: {3}, ClaimsCount: {4}, TotalAmount: {5}, Url: {6}",
                FileReference, EliteCZResult.JoinParties(Debtors), Representative, PublishDate, ClaimsCount, TotalAmount, Url);
        }
    }

    public class CreditScore
    {
        public string Name { get; set; }
        public CreditScoreValue[] Values { get; set; }

        public override string ToString()
        {
            var vals = new System.Collections.Generic.List<string>();
            if (Values != null)
            {
                foreach (var v in Values)
                {
                    vals.Add(string.Format("{0}:{1} {2}", v.Year, v.Value, v.State));
                }
            }
            return string.Format("{0}: [{1}]", Name, string.Join(",", vals.ToArray()));
        }
    }

    public class CreditScoreValue
    {
        public int Year { get; set; }
        public decimal? Value { get; set; }
        public CreditScoreStateEnum? State { get; set; }
    }

    public class RZPPerson
    {
        public enum FunctionEnum
        {
            Entrepreneur,
            ResponsiblePerson,
        }

        public string FullName { get; set; }
        public DateTime? BirthDate { get; set; }
        public FunctionEnum Function { get; set; }
        public DateTime? FunctionFrom { get; set; }
        // trade licences the responsible person answers for, empty for Entrepreneur
        public string[] Subjects { get; set; }

        public override string ToString()
        {
            return string.Format("{0} ({1:d}), Function: {2}, From: {3:d}, Subjects: {4}", FullName, BirthDate, Function, FunctionFrom, Subjects != null ? string.Join(", ", Subjects) : null);
        }
    }

    public class ORCRPerson : Address
    {
        public string FullName { get; set; }
        public DateTime? DetectedFrom { get; set; }
        public DateTime? DetectedTo { get; set; }
        public NameParts StructuredName { get; set; }
        public FunctionAssigment[] Functions { get; set; }
        public DateTime? BirthDate { get; set; }
        public decimal? DepositAmount { get; set; }
        public decimal? PaybackRange { get; set; }
        public decimal? PartnersSharePercentage { get; set; }
        // filled when the person is a company
        public string Ico { get; set; }

        public override string ToString()
        {
            StringBuilder dataString = new StringBuilder();
            dataString.AppendLine(string.Format("FullName: {0}", FullName));
            dataString.AppendLine(string.Format("Ico: {0}", Ico));
            dataString.AppendLine(string.Format("BirthDate: {0}", BirthDate));
            dataString.AppendLine(string.Format("StructuredName: {0}", StructuredName));
            dataString.AppendLine(base.ToString());
            dataString.AppendLine(string.Format("DetectedFrom: {0}", DetectedFrom));
            dataString.AppendLine(string.Format("DetectedTo: {0}", DetectedTo));
            dataString.AppendLine(string.Format("Functions: {0}", Functions != null ? string.Join(", ", Array.ConvertAll(Functions, x => x?.ToString())) : null));
            dataString.AppendLine(string.Format("DepositAmount: {0}", DepositAmount));
            dataString.AppendLine(string.Format("PaybackRange: {0}", PaybackRange));
            dataString.AppendLine(string.Format("PartnersSharePercentage: {0}%", PartnersSharePercentage));
            return dataString.ToString();
        }
    }

    public class FunctionAssigment
    {
        public string Type { get; set; }
        public string Description { get; set; }
        public DateTime? From { get; set; }

        public override string ToString()
        {
            return string.Format("Type: {0}, Description: {1}, From: {2}", Type, Description, From);
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
            return string.Format("Prefix: {0}, Name: {1}, Suffix: {2}, After: {3}",
                Prefix != null ? string.Join(" ", Prefix) : null,
                Name != null ? string.Join(" ", Name) : null,
                Suffix != null ? string.Join(" ", Suffix) : null,
                After != null ? string.Join(" ", After) : null);
        }
    }

    public class Subject
    {
        public string Title { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? SuspendedFrom { get; set; }
        public DateTime? SuspendedTo { get; set; }

        public override string ToString()
        {
            return string.Format("Title: {0}, ValidFrom: {1}, SuspendedFrom: {2}, SuspendedTo: {3}", Title, ValidFrom, SuspendedFrom, SuspendedTo);
        }
    }
}
