using System;
using System.Collections.Generic;
using System.Text;

namespace FinstatApi
{
    public class EliteCZResult : PremiumCZResult
    {
        public IsirResult[] Isir { get; set; }
        public IndicatorResult[] FinancialIndicators { get; set; }
        public string TurnoverCategory { get; set; }
        public string TurnoverCategoryCode { get; set; }

        public override string ToString()
        {
            StringBuilder dataString = new StringBuilder();
            dataString.AppendLine(base.ToString());
            dataString.AppendLine(string.Format("TurnoverCategory: {0}", TurnoverCategory));
            dataString.AppendLine(string.Format("TurnoverCategoryCode: {0}", TurnoverCategoryCode));
            if (Isir != null && Isir.Length > 0)
            {
                dataString.AppendLine("Isir:");
                foreach (var item in Isir)
                {
                    dataString.AppendLine(string.Format(" - LastChange: {0}, ReceivablesCount: {1}, ReceivablesAmount: {2}, Type: {3}", item.LastChange, item.ReceivablesCount, item.ReceivablesAmount, item.Type));
                }
            }
            if (FinancialIndicators != null && FinancialIndicators.Length > 0)
            {
                dataString.AppendLine("FinancialIndicators:");
                foreach (var indicator in FinancialIndicators)
                {
                    var vals = new List<string>();
                    if (indicator.Values != null)
                    {
                        foreach (var v in indicator.Values)
                        {
                            vals.Add(string.Format("{0}:{1}", v.Year, v.Value));
                        }
                    }
                    dataString.AppendLine(string.Format(" - {0}: [{1}]", indicator.Name, string.Join(",", vals.ToArray())));
                }
            }
            return dataString.ToString();
        }
    }

    public class IsirResult
    {
        public DateTime? LastChange { get; set; }
        public int? ReceivablesCount { get; set; }
        public decimal? ReceivablesAmount { get; set; }
        public string Type { get; set; }
    }

    public class IndicatorResult
    {
        public string Name { get; set; }
        public IndicatorValue[] Values { get; set; }
    }

    public class IndicatorValue
    {
        public int Year { get; set; }
        public decimal? Value { get; set; }
    }
}