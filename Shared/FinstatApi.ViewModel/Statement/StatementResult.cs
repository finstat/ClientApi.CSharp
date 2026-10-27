using System;
using System.Collections.Generic;
using System.Text;

namespace FinstatApi.Statement
{
    // the server's numbers: JSON sends templates as numbers (XML by name), and the server's enum starts with
    // Unsupported = 0 and has CZ and analysis templates between TemplateFinancial and TemplateROPO
    public enum TemplateTypeEnum
    {
        Template2011v2 = 1,
        Template2014 = 2,
        Template2014micro = 3,
        TemplateFinancial = 4,
        TemplateROPO = 8,
        TemplateNujPU = 9
    }

    public abstract class AbstractStatementLegendResult
    {
        public StatementLegendValue[] Assets { get; set; }
        public StatementLegendValue[] LiabilitiesAndEquity { get; set; }
    }

    public class StatementLegendResult : AbstractStatementLegendResult
    {
        public StatementLegendValue[] IncomeStatement { get; set; }
    }

    public class NonProfitStatementLegendResult : AbstractStatementLegendResult
    {
        public StatementLegendValue[] Expenses { get; set; }
        public StatementLegendValue[] Revenue { get; set; }
    }

    public class StatementLegendValue
    {
        public string ReportRow { get; set; }
        public string ReportSection { get; set; }
        public string Name { get; set; }
    }

    public class StatementItem
    {
        public int Year { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public DateTime? DatePublished { get; set; }
        public TemplateTypeEnum[] Templates { get; set; }
    }

    public class StatementValue
    {
        public string ReportRow { get; set; }
        public string ReportSection { get; set; }
        public double? Actual { get; set; }
        public double? Previous { get; set; }
    }

    public class AssetStatementValue : StatementValue
    {
        public double? ActualBrutto { get; set; }
        public double? ActualCorrection { get; set; }
    }

    public class FinancialStatementValue : StatementValue
    {
        public double? ActualMain { get; set; }
        public double? ActualCommercial { get; set; }
    }

    public abstract class AbstractStatementResult
    {
        public string ICO { get; set; }
        public string Name { get; set; }
        public int Year { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public DateTime? DatePublished { get; set; }
        public TemplateTypeEnum Format { get; set; }
        public TemplateTypeEnum OriginalFormat { get; set; }
        public string Source { get; set; }
        public AssetStatementValue[] Assets { get; set; }
        public StatementValue[] LiabilitiesAndEquity { get; set; }
        public DateTime? PreviousAccountingPeriodFrom { get; set; }
        public DateTime? PreviousAccountingPeriodTo { get; set; }
    }

    public class StatementResult : AbstractStatementResult
    {
        public StatementValue[] IncomeStatement { get; set; }
    }

    public class NonProfitStatementResult : AbstractStatementResult
    {
        public FinancialStatementValue[] Expenses { get; set; }
        public FinancialStatementValue[] Revenue { get; set; }
    }
}
