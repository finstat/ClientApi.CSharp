using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace FinstatApi
{
    public class EliteCZResult : PremiumCZResult
    {
        public ContactItem[] Contacts { get; set; }
        public CzNace[] OtherCzNaces { get; set; }
        public Offices Offices { get; set; }
        public VehicleFleet VehicleFleet { get; set; }
        public IsirProceeding[] IsirProceedings { get; set; }
        // 2 years in EliteCZ, 4 years in UltimateCZ
        public IndicatorResult[] FinancialIndicators { get; set; }

        public override string ToString()
        {
            StringBuilder dataString = new StringBuilder();
            dataString.AppendLine(base.ToString());
            AppendItems(dataString, "Contacts", Contacts);
            AppendItems(dataString, "OtherCzNaces", OtherCzNaces);
            if (Offices != null)
            {
                dataString.AppendLine("Offices:");
                dataString.AppendLine(Offices.ToString());
            }
            if (VehicleFleet != null)
            {
                dataString.AppendLine("VehicleFleet:");
                dataString.AppendLine(VehicleFleet.ToString());
            }
            AppendItems(dataString, "IsirProceedings", IsirProceedings);
            AppendItems(dataString, "FinancialIndicators", FinancialIndicators);
            return dataString.ToString();
        }

        internal static void AppendItems<T>(StringBuilder dataString, string name, IEnumerable<T> items)
        {
            if (items == null)
            {
                return;
            }
            dataString.AppendLine(string.Format("{0}:", name));
            foreach (var item in items)
            {
                dataString.AppendLine(string.Format(" - {0}", item));
            }
        }

        internal static string JoinParties(IsirParty[] parties)
        {
            return parties != null ? string.Join("; ", Array.ConvertAll(parties, x => x?.ToString())) : null;
        }
    }

    // class cannot be named Contact (member Contact), XML element keeps the name from the spec
    [XmlType("Contact")]
    public class ContactItem
    {
        // EliteCZ returns Email and Phone, UltimateCZ adds web pages and social networks
        public enum ContactTypeEnum
        {
            Email,
            Phone,
            Web,
            LinkedIn,
            Facebook,
            Instagram,
            X,
            YouTube,
            TikTok,
        }

        public string Contact { get; set; }
        public ContactTypeEnum Type { get; set; }
        public string[] Sources { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1} (Sources: {2})", Type, Contact, Sources != null ? string.Join(", ", Sources) : null);
        }
    }

    public class CzNace
    {
        public string Code { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1}", Code, Text);
        }
    }

    public class Offices
    {
        public int Count { get; set; }
        // UltimateCZ only
        public Office[] Items { get; set; }

        public override string ToString()
        {
            StringBuilder dataString = new StringBuilder();
            dataString.AppendLine(string.Format("Count: {0}", Count));
            EliteCZResult.AppendItems(dataString, "Items", Items);
            return dataString.ToString();
        }
    }

    public class Office : Address
    {
        public string[] Subjects { get; set; }
        public OfficeType Type { get; set; }
        // UltimateCZ only (RŽP office name)
        public string Name { get; set; }

        public override string ToString()
        {
            StringBuilder dataString = new StringBuilder();
            dataString.AppendLine(string.Format("Name: {0}", Name));
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

    public class VehicleFleet
    {
        public int Year { get; set; }
        public int ActiveCount { get; set; }
        public string Url { get; set; }

        // UltimateCZ only, breakdowns of VehicleCount for Year
        public int? VehicleCount { get; set; }
        public VehicleFleetItem[] OwnershipTypes { get; set; }
        public VehicleFleetItem[] VehicleTypes { get; set; }
        public VehicleFleetItem[] ProductionYears { get; set; }
        public VehicleFleetBrand[] TopBrands { get; set; }

        public override string ToString()
        {
            StringBuilder dataString = new StringBuilder();
            dataString.AppendLine(string.Format("Year: {0}", Year));
            dataString.AppendLine(string.Format("ActiveCount: {0}", ActiveCount));
            dataString.AppendLine(string.Format("Url: {0}", Url));
            if (VehicleCount != null)
            {
                dataString.AppendLine(string.Format("VehicleCount: {0}", VehicleCount));
            }
            EliteCZResult.AppendItems(dataString, "OwnershipTypes", OwnershipTypes);
            EliteCZResult.AppendItems(dataString, "VehicleTypes", VehicleTypes);
            EliteCZResult.AppendItems(dataString, "ProductionYears", ProductionYears);
            EliteCZResult.AppendItems(dataString, "TopBrands", TopBrands);
            return dataString.ToString();
        }
    }

    public class VehicleFleetItem
    {
        public string Type { get; set; }
        public int Count { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Type, Count);
        }
    }

    public class VehicleFleetBrand
    {
        public string Name { get; set; }
        public int Count { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Name, Count);
        }
    }

    public class IsirProceeding
    {
        public string FileReference { get; set; }
        public string Url { get; set; }
        public string Court { get; set; }
        public DateTime FirstEvent { get; set; }
        public DateTime LastEvent { get; set; }
        public string Status { get; set; }
        public IsirParty[] Debtors { get; set; }

        // UltimateCZ only
        public string Type { get; set; }
        public DateTime? StartDate { get; set; }
        public IsirParty[] Officers { get; set; }
        public int? CreditorsCount { get; set; }
        public IsirParty[] Creditors { get; set; }
        public decimal? AmountOfClaims { get; set; }
        public ReceivableApplication[] ReceivableApplications { get; set; }

        public override string ToString()
        {
            StringBuilder dataString = new StringBuilder();
            dataString.AppendLine(string.Format("FileReference: {0}, Court: {1}, Status: {2}", FileReference, Court, Status));
            dataString.AppendLine(string.Format("   FirstEvent: {0}, LastEvent: {1}, Url: {2}", FirstEvent, LastEvent, Url));
            dataString.AppendLine(string.Format("   Debtors: {0}", EliteCZResult.JoinParties(Debtors)));
            if (Type != null || StartDate != null || Officers != null || CreditorsCount != null || Creditors != null || AmountOfClaims != null || ReceivableApplications != null)
            {
                dataString.AppendLine(string.Format("   Type: {0}, StartDate: {1}, CreditorsCount: {2}, AmountOfClaims: {3}", Type, StartDate, CreditorsCount, AmountOfClaims));
                dataString.AppendLine(string.Format("   Officers: {0}", EliteCZResult.JoinParties(Officers)));
                dataString.AppendLine(string.Format("   Creditors: {0}", EliteCZResult.JoinParties(Creditors)));
                if (ReceivableApplications != null)
                {
                    dataString.AppendLine("   ReceivableApplications:");
                    foreach (var item in ReceivableApplications)
                    {
                        dataString.AppendLine(string.Format("    - {0}", item));
                    }
                }
            }
            return dataString.ToString();
        }
    }

    /// <summary>
    /// Claim application filed in an ISIR proceeding.
    /// </summary>
    public class ReceivableApplication
    {
        public IsirParty Creditor { get; set; }
        // creditor's representative has no birth date in ISIR, only Name and Ico are filled
        public IsirParty Representative { get; set; }
        public DateTime PublishDate { get; set; }
        // null when the application document was not scanned reliably
        public int? ClaimsCount { get; set; }
        public decimal? TotalAmount { get; set; }

        public override string ToString()
        {
            return string.Format("Creditor: {0}, Representative: {1}, PublishDate: {2}, ClaimsCount: {3}, TotalAmount: {4}", Creditor, Representative, PublishDate, ClaimsCount, TotalAmount);
        }
    }

    /// <summary>
    /// Any person in ISIR (debtor, creditor, officer, representative): company has Ico, natural person BirthDate.
    /// </summary>
    public class IsirParty
    {
        public string Name { get; set; }
        public string Ico { get; set; }
        public DateTime? BirthDate { get; set; }

        public override string ToString()
        {
            return Ico != null
                ? string.Format("{0} (IČO {1})", Name, Ico)
                : BirthDate != null ? string.Format("{0} ({1:d})", Name, BirthDate) : Name;
        }
    }

    [XmlType("Indicator")]
    public class IndicatorResult
    {
        public string Name { get; set; }
        public IndicatorValue[] Values { get; set; }

        public override string ToString()
        {
            var vals = new List<string>();
            if (Values != null)
            {
                foreach (var v in Values)
                {
                    vals.Add(string.Format("{0}:{1}", v.Year, v.Value));
                }
            }
            return string.Format("{0}: [{1}]", Name, string.Join(",", vals.ToArray()));
        }
    }

    public class IndicatorValue
    {
        public int Year { get; set; }
        public decimal? Value { get; set; }
    }
}
