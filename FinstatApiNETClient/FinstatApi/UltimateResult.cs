﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace FinstatApi
{
    public class UltimateResult : ExtendedResult
    {
        public string ORSection { get; set; }
        public string ORInsertNo { get; set; }
        public Person[] Persons { get; set; }

        public class Person
        {
            public string FullName { get; set; }
            public string Street { get; set; }
            public string StreetNumber { get; set; }
            public string ZipCode { get; set; }
            public string City { get; set; }
            public DateTime DetectedFrom { get; set; }
            public DateTime? DetectedTo { get; set; }
            public FunctionAssigment[] Functions { get; set; }
        }

        public class FunctionAssigment
        {
            public string Type { get; set; }
            public string Description { get; set; }
            public DateTime? From { get; set; }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            if (!string.IsNullOrEmpty(ORSection))
            {
                result.Append("\nSekcia: " + ORSection);
                result.Append(" Vlozka: " + ORInsertNo);
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
                    result.Append(string.Format("  Cele meno: {0}; Mesto: {1}; Funkcie: ", person.FullName, person.City));
                    foreach (var function in person.Functions)
                    {
                        result.Append(string.Format("{0} - {1}, ", function.Type, function.Description));    
                    }
                    result.AppendLine();
                }
            }
            return base.ToString() + result.ToString();
        }
    }
}