﻿using System;
using System.Text;

namespace FinstatApi
{
    public class UltimateResult : ExtendedResult
    {
        public string ORSection { get; set; }
        public string ORInsertNo { get; set; }
        public Person[] Persons { get; set; }
        public decimal? BasicCapital { get; set; }
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

        public class Person : Address
        {
            public string FullName { get; set; }
            public DateTime DetectedFrom { get; set; }
            public DateTime? DetectedTo { get; set; }
            public FunctionAssigment[] Functions { get; set; }
            public decimal? DepositAmount { get; set; }
            public decimal? PaybackRange { get; set; }
        }

        public class FunctionAssigment
        {
            public string Type { get; set; }
            public string Description { get; set; }
            public DateTime? From { get; set; }
        }

        public class Court: Address
        {
            public string Name { get; set; }
        }

        public class HistoryAddress : Address
        {
            public DateTime ValidFrom { get; set; }
            public DateTime? ValidTo { get; set; }
        }

        public class LiquidationResult
        {
            public DateTime? EnterDate { get; set; }
            public string EnterReason { get; set; }
            public DateTime? ExitDate { get; set; }
            public Person Officer { get; set; }
        }
        public class BankruptResult : LiquidationResult
        {
            public string ExitReason { get; set; }
        }

        public class RestructuringResult : BankruptResult
        {
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            if (!string.IsNullOrEmpty(ORSection))
            {
                result.Append("\nSekcia: " + ORSection);
                result.Append(" Vlozka: " + ORInsertNo);
            }
            if (ORCancelled != null && ORCancelled.HasValue)
            {
                result.Append("\nZrušená podľa OR: " + ORCancelled.Value.ToString("dd.MM.yyyy"));
            }
            if (RegistrationCourt != null)
            {
                result.Append("\nZapisany na: " + RegistrationCourt.Name);
            }
            if (!string.IsNullOrEmpty(LegalFormCode))
            {
                result.Append(string.Format("\nPravna forma: {0} [{1}]", LegalFormText, LegalFormCode ));
            }
            if (Persons == null || Persons.Length == 0)
            {
                result.Append("\nBez osôb");
            }
            else
            {
                result.AppendLine("\nOsoby:");
                foreach (var person in Persons)
                {
                    result.Append(string.Format("  Cele meno: {0}; Mesto: {1}; Okres: {2}; Funkcie: ", person.FullName, person.City, person.District));
                    foreach (var function in person.Functions)
                    {
                        result.Append(string.Format("{0} - {1}, ", function.Type, function.Description));
                    }
                    result.AppendLine();
                }
            }
            if(!string.IsNullOrEmpty(StatutoryAction))
            {
                result.AppendLine("\nKonanie statutarov: " + StatutoryAction);
            }
            if (!string.IsNullOrEmpty(ProcurationAction))
            {
                result.AppendLine("\nKonanie prokury: " + ProcurationAction);
            }
            if (WebPages != null && WebPages.Length > 0)
            {
                result.AppendLine("\nWeb stranky: " + string.Join(", ", WebPages));
            }
            if (AddressHistory!= null && AddressHistory.Length > 0)
            {
                result.AppendLine("\nHistoricke adresy (pocet): " + AddressHistory.Length);
            }
            if (Bankrupt!= null)
            {
                result.AppendLine(string.Format("\nKonkurz: {0:dd.MM.yyyy} - {1:dd.MM.yyyy}", Bankrupt.EnterDate, Bankrupt.ExitDate));
            }
            if (Restructuring != null)
            {
                result.AppendLine(string.Format("\nReštruktualizácia: {0:dd.MM.yyyy} - {1:dd.MM.yyyy}", Restructuring.EnterDate, Restructuring.ExitDate));
            }
            if (Liquidation != null)
            {
                result.AppendLine(string.Format("\nLikvidácia: {0:dd.MM.yyyy} - {1:dd.MM.yyyy}", Liquidation.EnterDate, Liquidation.ExitDate));
            }
            return base.ToString() + result;
        }
    }
}