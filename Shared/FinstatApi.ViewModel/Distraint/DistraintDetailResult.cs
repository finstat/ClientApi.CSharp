using System;
using System.Collections.Generic;
using System.Text;

namespace FinstatApi
{
    public class DistraintDetailResult : DistraintPreview
    {
        public string Court { get; set; }               // Sud
        public DateTime DateOfAuthorisation { get; set; }      // DatumPoverenia
        public string CourtCode { get; set; }           // SudZn
        public string EnforcementDetails { get; set; }    // PopisNaroku
        public decimal SumOutstanding { get; set; }             // VyskaPlnenia
        public string Currency { get; set; }            // Mena
        public Bailiff Bailiff { get; set; }                      // Exekutor

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine(base.ToString());

            result.AppendLine(string.Format("Court: {0} DateOfAuthorisation: {1} CourtCode: {2} SumOutstanding: {3} Currency: {4}",
                Court, DateOfAuthorisation, CourtCode, SumOutstanding, Currency));
            if (Bailiff != null)
            {
                result.AppendLine(Bailiff.ToString());
            }
            result.AppendLine(string.Format("ClaimDescription: {0}",
                EnforcementDetails));
            return result.ToString();
        }
    }

    public class DistraintDetailResults
    {
        public DistraintDetailResult[] DistraintDetails { get; set; }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine(base.ToString());

            if (DistraintDetails != null && DistraintDetails.Length > 0)
            {
                result.AppendLine("\nDistraints:");
                foreach (var distraint in DistraintDetails)
                {
                    result.AppendLine(distraint.ToString());
                }
            }
            return result.ToString();
        }
    }
}
