using System;
using System.Collections.Generic;
using System.Text;

namespace FinstatApi
{
    public class UltimateResult : ExtendedResult
    {
        public int? EmployeesNumber { get; set; }
        public string ORSection { get; set; }
        public string ORInsertNo { get; set; }
        public Person[] Persons { get; set; }
        public RpvsPerson[] RpvsPersons { get; set; }
        public decimal? PaybackRange { get; set; }
        public Court RegistrationCourt { get; set; }
        public string[] WebPages { get; set; }
        public HistoryAddress[] AddressHistory { get; set; }
        public string StatutoryAction { get; set; }
        public string ProcurationAction { get; set; }
        public BankruptResult Bankrupt { get; set; }
        public RestructuringResult Restructuring { get; set; }
        public LiquidationResult Liquidation { get; set; }
        public DateTime? ORCancelled { get; set; }
        public ProceedingResult OtherProceeding { get; set; }
        public PreventiveRestructuringResult PreventiveRestructuring { get; set; }
        public RPOPerson[] RPOPersons { get; set; }
        public DistraintsAuthorizationDetail[] DistraintsAuthorizations { get; set; }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine(base.ToString());
            if (EmployeesNumber.HasValue)
            {
                result.AppendLine(string.Format("EmployeesNumber: {0}", EmployeesNumber.Value));
            }
            if (!string.IsNullOrEmpty(ORSection))
            {
                //sekicam vlozka
                result.AppendLine(string.Format("ORSection: {0} ORInsertNo: {1}", ORSection, ORInsertNo));
            }
            if (ORCancelled != null && ORCancelled.HasValue)
            {
                //Zrušená podľa OR
                result.AppendLine(string.Format("ORCancelled: {0}", ORCancelled));
            }
            if (RegistrationCourt != null)
            {
                //nZapisany na
                result.AppendLine(string.Format("RegistrationCourt: {0}", RegistrationCourt.Name));
            }
            if (Persons != null && Persons.Length > 0)
            {
                result.AppendLine("\nPerson:");
                foreach (var person in Persons)
                {
                    result.AppendLine(person.ToString());
                }
            }
            if (RpvsPersons != null && RpvsPersons.Length > 0)
            {
                result.AppendLine("\nRPVS Person:");
                foreach (var person in RpvsPersons)
                {
                    result.AppendLine(person.ToString());
                }
            }
            if (!string.IsNullOrEmpty(StatutoryAction))
            {
                result.AppendLine(string.Format("StatutoryAction: {0}", StatutoryAction));
            }
            if (!string.IsNullOrEmpty(ProcurationAction))
            {
                result.AppendLine(string.Format("ProcurationAction: {0}", ProcurationAction));
            }
            if (WebPages != null && WebPages.Length > 0)
            {
                result.AppendLine(string.Format("WebPages: {0}", string.Join(", ", WebPages)));
            }
            if (AddressHistory != null && AddressHistory.Length > 0)
            {
                result.AppendLine(string.Format("AddressHistory (count): {0}", AddressHistory.Length));
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
            if (PreventiveRestructuring != null)
            {
                result.AppendLine(string.Format("PreventiveRestructuring: {0}", PreventiveRestructuring));
            }
            if (DistraintsAuthorizations != null && DistraintsAuthorizations.Length > 0)
            {
                result.AppendLine(string.Format("DistraintsAuthorizations: {0}", DistraintsAuthorizations.Length));
            }
            return result.ToString();
        }

        public class AbstractPerson : Address
        {
            public string FullName { get; set; }
            public DateTime? DetectedFrom { get; set; }
            public DateTime? DetectedTo { get; set; }
            public FunctionAssigment[] Functions { get; set; }
            public NameParts StructuredName { get; set; }

            public override string ToString()
            {
                StringBuilder dataString = new StringBuilder();
                dataString.AppendLine(string.Format("FullName: {0}", FullName));
                dataString.AppendLine(string.Format("StructuredName: {0}", StructuredName));
                dataString.AppendLine(base.ToString());
                dataString.AppendLine(string.Format("DetectedFrom: {0}", DetectedFrom));
                dataString.AppendLine(string.Format("DetectedTo: {0}", DetectedTo));
                var f = new StringBuilder();
                int i = 0;
                if (Functions != null && Functions.Length > 0)
                {
                    foreach (var item in Functions)
                    {
                        if (i > 0)
                        {
                            f.Append(", ");
                        }
                        f.Append(item);
                        i++;
                    }
                }
                dataString.AppendLine(string.Format("Functions (0): {1}", i, f));
                return dataString.ToString();
            }
        }

        public class AbstractPersonBirthDate : AbstractPerson
        {
            public DateTime? BirthDate { get; set; }

            public override string ToString()
            {
                StringBuilder dataString = new StringBuilder();
                dataString.AppendLine(base.ToString());
                dataString.AppendLine(string.Format("BirthDate: {0}", BirthDate));
                return dataString.ToString();
            }
        }

        public class Person : AbstractPersonBirthDate
        {
            public decimal? DepositAmount { get; set; }
            public decimal? PaybackRange { get; set; }
            public decimal? PartnersSharePercentage { get; set; }

            public override string ToString()
            {
                StringBuilder dataString = new StringBuilder();
                dataString.AppendLine(base.ToString());
                dataString.AppendLine(string.Format("DepositAmount: {0}", DepositAmount));
                dataString.AppendLine(string.Format("PaybackRange: {0}", PaybackRange));
                dataString.AppendLine(string.Format("PartnersSharePercentage: {0}%", PartnersSharePercentage));
                return dataString.ToString();
            }
        }

        public class Officer : AbstractPerson
        {
            public Source Source { get; set; }
            public override string ToString()
            {
                StringBuilder dataString = new StringBuilder();
                dataString.AppendLine(base.ToString());
                dataString.AppendLine(string.Format("Source {0}", Source));
                return dataString.ToString();
            }
        }

        public class RpvsPerson : AbstractPersonBirthDate
        {
            public string Ico { get; set; }

            public override string ToString()
            {
                StringBuilder dataString = new StringBuilder();
                dataString.AppendLine(string.Format("BirthDate: {0}", BirthDate));
                dataString.AppendLine(string.Format("Ico: {0}", Ico));
                dataString.AppendLine(base.ToString());
                return dataString.ToString();
            }
        }

        public class RPOPerson
        {
            public DateTime? BirthDate { get; set; }
            public string Citizenship { get; set; }
            public string FullName { get; set; }
            public string Country { get; set; }
            public DateTime? DetectedFrom { get; set; }
            public DateTime? DetectedTo { get; set; }
            public FunctionAssigment[] Functions { get; set; }
            public NameParts StructuredName { get; set; }

            public override string ToString()
            {
                StringBuilder dataString = new StringBuilder();
                dataString.AppendLine(string.Format("FullName: {0}", FullName));
                dataString.AppendLine(string.Format("Country: {0}", Country));
                dataString.AppendLine(string.Format("StructuredName: {0}", StructuredName));
                dataString.AppendLine(string.Format("DetectedFrom: {0}", DetectedFrom));
                dataString.AppendLine(string.Format("DetectedTo: {0}", DetectedTo));
                var f = new StringBuilder();
                int i = 0;
                if (Functions != null && Functions.Length > 0)
                {
                    foreach (var item in Functions)
                    {
                        if (i > 0)
                        {
                            f.Append(", ");
                        }
                        f.Append(item);
                        i++;
                    }
                }
                dataString.AppendLine(string.Format("Functions (0): {1}", i, f));
                dataString.AppendLine(string.Format("BirthDate: {0}", BirthDate));
                dataString.AppendLine(string.Format("Citizenship: {0}", Citizenship));
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
                StringBuilder dataString = new StringBuilder();
                dataString.Append(string.Format("Type: {0}", Type));
                dataString.Append(string.Format("Description: {0}", Description));
                dataString.Append(string.Format("From: {0}", From));
                return dataString.ToString();
            }
        }

        public class Court : Address
        {
            public string Name { get; set; }

            public override string ToString()
            {
                StringBuilder dataString = new StringBuilder();
                dataString.AppendLine(string.Format("Name: {0}", Name));
                dataString.AppendLine(base.ToString());
                return dataString.ToString();
            }
        }

        public class HistoryAddress : Address
        {

            public DateTime ValidFrom { get; set; }
            public DateTime? ValidTo { get; set; }
            public override string ToString()
            {
                StringBuilder dataString = new StringBuilder();
                dataString.Append(base.ToString());
                dataString.AppendLine(string.Format("ValidFrom: {0}", ValidFrom));
                dataString.AppendLine(string.Format("ValidTo: {0}", ValidTo));
                return dataString.ToString();
            }
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

        public class LiquidationResult
        {
            public DateTime? EnterDate { get; set; }
            public string EnterReason { get; set; }
            public DateTime? ExitDate { get; set; }
            public Officer[] Officers { get; set; }
            public Source Source { get; set; }
            public Deadline[] Deadlines { get; set; }

            public override string ToString()
            {
                StringBuilder dataString = new StringBuilder();
                dataString.Append(string.Format("EnterDate: {0}", EnterDate));
                dataString.Append(string.Format("EnterReason: {0}", EnterReason));
                dataString.Append(string.Format("ExitDate: {0}", ExitDate));
                dataString.Append(string.Format("Officers: {0}", Officers?.Length));
                dataString.AppendLine(string.Format("Source: {0}", Source));
                var f = new StringBuilder();
                int i = 0;
                if (Deadlines != null && Deadlines.Length > 0)
                {
                    foreach (var item in Deadlines)
                    {
                        if (i > 0)
                        {
                            f.Append(", ");
                        }
                        f.Append(item);
                        i++;
                    }
                }
                dataString.AppendLine(string.Format("Deadlines (0): {1}", i, f));
                return dataString.ToString();
            }
        }

        public class ProceedingResult : LiquidationResult
        {
            public DateTime? StartDate { get; set; }
            public string ExitReason { get; set; }
            public string Status { get; set; }
            public string FileReference { get; set; }
            public string CourtCode { get; set; }

            public override string ToString()
            {
                StringBuilder dataString = new StringBuilder();
                dataString.Append(base.ToString());
                dataString.Append(string.Format("FileReference: {0}", FileReference));
                dataString.Append(string.Format("CourtCode: {0}", CourtCode));
                dataString.Append(string.Format("ExitReason: {0}", ExitReason));
                dataString.Append(string.Format("Status: {0}", Status));
                return dataString.ToString();
            }
        }

        public class RestructuringResult : ProceedingResult
        {
        }

        public class BankruptResult : ProceedingResult
        {
        }

        public class PreventiveRestructuringResult
        {
            public string FileReference { get; set; }
            public string CourtCode { get; set; }
            public Source? Source { get; set; }
            public DateTime? FirstDate { get; set; }
            public DateTime? LastDate { get; set; }

            public override string ToString()
            {
                StringBuilder dataString = new StringBuilder();
                dataString.Append(base.ToString());
                dataString.Append(string.Format("FileReference: {0}", FileReference));
                dataString.Append(string.Format("CourtCode: {0}", CourtCode));
                dataString.Append(string.Format("FirstDate: {0}", FirstDate));
                dataString.Append(string.Format("LastDate: {0}", LastDate));
                return dataString.ToString();
            }
        }

        public class DistraintsAuthorizationDetail
        {
            public string ReferenceNumber { get; set; }
            public BaseInfo[] Authorized { get; set; }
            public string TypeOfClaim { get; set; }
            public string Plaintiff { get; set; }
            public DateTime PublishDate { get; set; }
            public string Url { get; set; }
            public string Court { get; set; }
            public string IdentifierNumber { get; set; }

            public override string ToString()
            {
                StringBuilder dataString = new StringBuilder();
                dataString.Append(base.ToString());
                dataString.Append(string.Format("ReferenceNumber: {0}", ReferenceNumber));
                dataString.Append(string.Format("TypeOfClaim: {0}", TypeOfClaim));
                dataString.Append(string.Format("Authorized: {0}", (Authorized != null && Authorized.Length > 0) ? Authorized.Length : 0));
                dataString.Append(string.Format("Plaintiff: {0}", Plaintiff));
                dataString.Append(string.Format("PublishDate: {0}", PublishDate));
                dataString.Append(string.Format("Url: {0}", Url));
                dataString.Append(string.Format("Court: {0}", Court));
                dataString.Append(string.Format("IdentifierNumber: {0}", IdentifierNumber));
                return dataString.ToString();
            }
        }
    }
}
