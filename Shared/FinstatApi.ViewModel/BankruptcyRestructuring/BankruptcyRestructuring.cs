using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace FinstatApi
{
    public class BankruptcyRestructuring
    {
        public PersonAddress[] Debtors { get; set; }
        public string FileReference { get; set; }
        public DateTime? FirstRecordDate { get; set; }
        public DateTime? LastRecordDate { get; set; }
        public string RUState { get; set; }
        public DateTime? RUStateDate { get; set; }
        public string OVState { get; set; }
        public DateTime? OVStateDate { get; set; }
        public DateTime? EnterDate { get; set; }
        public DateTime? ExitDate { get; set; }
        public string EndState { get; set; }
        public string EndReason { get; set; }
        public Deadline[] Deadlines { get; set; }
        public string FinstatURL { get; set; }

        public override string ToString()
        {
            StringBuilder dataString = new StringBuilder();

            //dataString.AppendLine(base.ToString());
            dataString.AppendLine(string.Format("Debtors: {0}", string.Join("; ", Debtors?.Select(x => x.ToString()) ?? Array.Empty<string>())));
            dataString.AppendLine(string.Format("FileReference: {0}", FileReference));
            dataString.AppendLine(string.Format("FirstRecordDate: {0:yyyy-MM-dd}", FirstRecordDate));
            dataString.AppendLine(string.Format("LastRecordDate: {0:yyyy-MM-dd}", LastRecordDate));
            dataString.AppendLine(string.Format("RUState: {0}", RUState));
            dataString.AppendLine(string.Format("RUStateDate: {0:yyyy-MM-dd}", RUStateDate));
            dataString.AppendLine(string.Format("OVState: {0}", OVState));
            dataString.AppendLine(string.Format("OVStateDate: {0:yyyy-MM-dd}", OVStateDate));
            dataString.AppendLine(string.Format("EnterDate: {0:yyyy-MM-dd}", EnterDate));
            dataString.AppendLine(string.Format("ExitDate: {0:yyyy-MM-dd}", ExitDate));
            dataString.AppendLine(string.Format("EndState: {0}", EndState));
            dataString.AppendLine(string.Format("EndReason: {0}", EndReason));
            dataString.AppendLine(string.Format("Deadlines: {0}", string.Join("; ", Deadlines?.Select(x => x.ToString()) ?? Array.Empty<string>())));
            dataString.AppendLine(string.Format("FinstatURL: {0}", FinstatURL));
            return dataString.ToString();
        }
    }
}