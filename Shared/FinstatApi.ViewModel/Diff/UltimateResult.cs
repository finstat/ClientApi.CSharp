using System;
using System.Collections.Generic;
using System.Text;

namespace FinstatApi.ViewModel.Diff
{
    public class UltimateResult : ExtendedResult
    {
        public string Country { get; set; }
        public DateTime? ORCancelled { get; set; }
        public DateTime? ORRemoved { get; set; }
        public bool SelfEmployed { get; set; }
        public NameParts StructuredName { get; set; }
        public string ORSection { get; set; }
        public string ORInsertNo { get; set; }
        public Court RegistrationCourt { get; set; }
        public string ProcurationAction { get; set; }
        public string StatutoryAction { get; set; }
        public Office[] Offices { get; set; }
        public ContactSource[] ContactSources { get; set; }
        public string[] WebPages { get; set; }
        public int? EmployeesNumber { get; set; }
        public Subject[] Subjects { get; set; }
        public HistoryAddress[] AddressHistory;
        public Person[] Persons { get; set; }
        public string RpvsInsert { get; set; }
        public string RpvsUrl { get; set; }
        public RpvsPerson[] RpvsPersons { get; set; }
        public bool HasDebt { get; set; }
        public string DebtUrl { get; set; }
        public bool HasDisposal { get; set; }
        public string DisposalUrl { get; set; }
        public DateTime? LiqDeadline { get; set; } // asi netreba
        public string KarUrl { get; set; }
        public BankruptResult Bankrupt { get; set; }
        public RestructuringResult Restructuring { get; set; }
        public LiquidationResult Liquidation { get; set; }
        public UltimateResult.ProceedingResult OtherProceeding { get; set; }
        public JudgementIndicator[] JudgementIndicators { get; set; } // netreba
        public string JudgementFinstatLink { get; set; }
        public JudgementCount[] JudgementCounts { get; set; }
        public DateTime? JudgementLastPublishedDate { get; set; }
        public decimal? BasicCapital { get; set; }
        public decimal? PaybackRange { get; set; }
        public string SalesCategory { get; set; }
        public string EmployeeYear { get; set; }
        public DateTime? ORLiquidationDate { get; set; }
        public ReceivableDebt[] StateReceivables { get; set; }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine(base.ToString());
            result.AppendLine(string.Format("Country: {0}", Country));
            if (ORCancelled.HasValue)
            {
                result.AppendLine(string.Format("ORCancelled: {0}", ORCancelled));
            }
            if (ORRemoved.HasValue)
            {
                result.AppendLine(string.Format("ORRemoved: {0}", ORRemoved));
            }
            result.AppendLine(string.Format("SelfEmployed: {0}", SelfEmployed));
            if (StructuredName != null)
            {
                result.AppendLine(string.Format("StructuredName: Prefix={0} Name={1} Suffix={2} After={3}",
                    StructuredName.Prefix != null ? string.Join(" ", StructuredName.Prefix) : null,
                    StructuredName.Name != null ? string.Join(" ", StructuredName.Name) : null,
                    StructuredName.Suffix != null ? string.Join(" ", StructuredName.Suffix) : null,
                    StructuredName.After != null ? string.Join(" ", StructuredName.After) : null));
            }
            if (!string.IsNullOrEmpty(ORSection))
            {
                result.AppendLine(string.Format("ORSection: {0} ORInsertNo: {1}", ORSection, ORInsertNo));
            }
            if (RegistrationCourt != null)
            {
                result.AppendLine(string.Format("RegistrationCourt: {0}", RegistrationCourt.Name));
            }
            if (!string.IsNullOrEmpty(ProcurationAction))
            {
                result.AppendLine(string.Format("ProcurationAction: {0}", ProcurationAction));
            }
            if (!string.IsNullOrEmpty(StatutoryAction))
            {
                result.AppendLine(string.Format("StatutoryAction: {0}", StatutoryAction));
            }
            if (Offices != null && Offices.Length > 0)
            {
                result.AppendLine(string.Format("Offices (count): {0}", Offices.Length));
            }
            if (ContactSources != null && ContactSources.Length > 0)
            {
                result.AppendLine(string.Format("ContactSources (count): {0}", ContactSources.Length));
            }
            if (WebPages != null && WebPages.Length > 0)
            {
                result.AppendLine(string.Format("WebPages: {0}", string.Join(", ", WebPages)));
            }
            if (EmployeesNumber.HasValue)
            {
                result.AppendLine(string.Format("EmployeesNumber: {0}", EmployeesNumber.Value));
            }
            if (Subjects != null && Subjects.Length > 0)
            {
                result.AppendLine(string.Format("Subjects (count): {0}", Subjects.Length));
            }
            if (AddressHistory != null && AddressHistory.Length > 0)
            {
                result.AppendLine(string.Format("AddressHistory (count): {0}", AddressHistory.Length));
            }
            if (Persons != null && Persons.Length > 0)
            {
                result.AppendLine(string.Format("Persons (count): {0}", Persons.Length));
            }
            if (!string.IsNullOrEmpty(RpvsInsert))
            {
                result.AppendLine(string.Format("RpvsInsert: {0} RpvsUrl: {1}", RpvsInsert, RpvsUrl));
            }
            if (RpvsPersons != null && RpvsPersons.Length > 0)
            {
                result.AppendLine(string.Format("RpvsPersons (count): {0}", RpvsPersons.Length));
            }
            result.AppendLine(string.Format("HasDebt: {0} DebtUrl: {1}", HasDebt, DebtUrl));
            result.AppendLine(string.Format("HasDisposal: {0} DisposalUrl: {1}", HasDisposal, DisposalUrl));
            if (LiqDeadline.HasValue)
            {
                result.AppendLine(string.Format("LiqDeadline: {0}", LiqDeadline));
            }
            if (!string.IsNullOrEmpty(KarUrl))
            {
                result.AppendLine(string.Format("KarUrl: {0}", KarUrl));
            }
            if (Bankrupt != null)
            {
                result.AppendLine(string.Format("Bankrupt: {0}", Bankrupt));
            }
            if (Restructuring != null)
            {
                result.AppendLine(string.Format("Restructuring: {0}", Restructuring));
            }
            if (Liquidation != null)
            {
                result.AppendLine(string.Format("Liquidation: {0}", Liquidation));
            }
            if (OtherProceeding != null)
            {
                result.AppendLine(string.Format("OtherProceeding: {0}", OtherProceeding));
            }
            if (JudgementIndicators != null && JudgementIndicators.Length > 0)
            {
                result.AppendLine(string.Format("JudgementIndicators (count): {0}", JudgementIndicators.Length));
            }
            if (!string.IsNullOrEmpty(JudgementFinstatLink))
            {
                result.AppendLine(string.Format("JudgementFinstatLink: {0}", JudgementFinstatLink));
            }
            if (JudgementCounts != null && JudgementCounts.Length > 0)
            {
                result.AppendLine(string.Format("JudgementCounts (count): {0}", JudgementCounts.Length));
            }
            if (JudgementLastPublishedDate.HasValue)
            {
                result.AppendLine(string.Format("JudgementLastPublishedDate: {0}", JudgementLastPublishedDate));
            }
            if (BasicCapital.HasValue)
            {
                result.AppendLine(string.Format("BasicCapital: {0}", BasicCapital));
            }
            if (PaybackRange.HasValue)
            {
                result.AppendLine(string.Format("PaybackRange: {0}", PaybackRange));
            }
            if (!string.IsNullOrEmpty(SalesCategory))
            {
                result.AppendLine(string.Format("SalesCategory: {0}", SalesCategory));
            }
            if (!string.IsNullOrEmpty(EmployeeYear))
            {
                result.AppendLine(string.Format("EmployeeYear: {0}", EmployeeYear));
            }
            if (ORLiquidationDate.HasValue)
            {
                result.AppendLine(string.Format("ORLiquidationDate: {0}", ORLiquidationDate));
            }
            if (StateReceivables != null && StateReceivables.Length > 0)
            {
                result.AppendLine(string.Format("StateReceivables (count): {0}", StateReceivables.Length));
            }
            return result.ToString();
        }

        public class Address
        {
            public string Street { get; set; }
            public string StreetNumber { get; set; }
            public string ZipCode { get; set; }
            public string City { get; set; }
            public string Country { get; set; }
            public string Region { get; set; }
            public string District { get; set; }
        }

        public class FullAddress : Address
        {
            public string Name { get; set; }
        }

        public class HistoryAddress : Address
        {
            public DateTime ValidFrom { get; set; }
            public DateTime? ValidTo { get; set; }
        }

        public class AbstractPerson : Address
        {
            public string FullName { get; set; }
            public DateTime? DetectedFrom { get; set; }
            public DateTime? DetectedTo { get; set; }
            public NameParts StructuredName { get; set; }
            public FunctionAssigment[] Functions { get; set; }
        }

        public class Office : Address
        {
            public string[] Subjects { get; set; }
            public OfficeType Type { get; set; }
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
        }

        public class Person : AbstractPerson
        {
            public decimal? DepositAmount { get; set; }
            public decimal? PaybackRange { get; set; }
            public string ICO { get; set; }
        }

        public class RpvsPerson : AbstractPerson
        {
            public DateTime? BirthDate { get; set; }
            public string ICO { get; set; }
        }

        public class FunctionAssigment
        {
            public string Type { get; set; }
            public string Description { get; set; }
            public DateTime? From { get; set; }
        }

        public class Court : FullAddress
        {
        }

        public class JudgementCount
        {
            public string Name { get; set; }
            public int? Value { get; set; }
        }

        public class JudgementIndicator
        {
            public string Name { get; set; }
            public bool? Value { get; set; }
        }

        public class Deadline
        {
            public string Type { get; set; }
            public DateTime Date { get; set; }
        }

        public enum Source
        {
            /// <summary>
            ///  Register úpadcov
            /// </summary>
            BankruptcyRegister,
            /// <summary>
            ///  Obchodný vestník
            /// </summary>
            CommercialBulletin,
            /// <summary>
            ///  Obchodný register SR
            /// </summary>
            CompaniesRegister,
        }

        public class BaseLiquidationResult
        {
            public DateTime? EnterDate { get; set; }
            public string EnterReason { get; set; }
            public DateTime? ExitDate { get; set; }
            public Person Officer { get; set; }
            public Source? Source { get; set; }
        }

        public class LiquidationResult : BaseLiquidationResult
        {
            public string DeleteReason { get; set; }
            public string CancelReason { get; set; }
            public string LiquidatorProcedure { get; set; }
            public string Deadline { get; set; }
        }

        public class ProceedingResult : BaseLiquidationResult
        {
            public DateTime? StartDate { get; set; }
            public string ExitReason { get; set; }
            public string Status { get; set; }
            public DateTime? ProposalDate { get; set; }
            public string FileReference { get; set; }
            public string CourtCode { get; set; }
            public DateTime? CourtPublishingDate { get; set; }
            public Deadline[] Deadlines { get; set; }
        }

        public class RestructuringResult : ProceedingResult
        {
        }

        public class BankruptResult : ProceedingResult
        {
        }

        public class ContactSource
        {
            public string Contact { get; set; }
            public string[] Sources { get; set; }
        }

        public class ReceivableDebt : Debt
        {
        }

        public class NameParts
        {
            public string[] Prefix { get; set; }
            public string[] Name { get; set; }
            public string[] Suffix { get; set; }
            public string[] After { get; set; }
        }
    }
}
